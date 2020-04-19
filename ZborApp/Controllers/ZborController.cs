using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ZborApp.Models;
using ZborApp.Models.JSONModels;
using ZborApp.Models.ZborViewModels;
using ZborApp.Services;
using ZborData.Account;
using ZborData.Model;



namespace ZborApp.Controllers
{
    [Authorize]
    public class ZborController : Controller
    {
        private readonly ILogger<ZborController> _logger;
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IHubContext<ChatHub> _hubContext;
        private const string LOCATION = "E:/UploadZbor/";

        public ZborController(ILogger<ZborController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
            _emailSender = new EmailSender();
            _hubContext = hubContext;
        }
        private bool Exists(Guid idZbor)
        {
            var zbor = _ctx.Zbor.Find(idZbor);
            return zbor == null ? false : true;
        }
        private bool CheckRights(Guid idZbor, Guid idKorisnik)
        {
            var clan = _ctx.ClanZbora.Where(c => c.IdKorisnik == idKorisnik && c.IdZbor == idZbor).SingleOrDefault();
            var voditelj = _ctx.Voditelj.Where(v => v.IdZbor == idZbor).OrderByDescending(v => v.DatumPostanka).SingleOrDefault();
            if (clan != null || voditelj.IdKorisnik == idKorisnik)
                return true;
            return false;
        }
        private bool IsAdmin(Guid idZbor, Guid idKorisnik)
        {
            var zbor = _ctx.Zbor.Where(z => z.Id == idZbor).Include(z => z.Voditelj).Include(z => z.ModeratorZbora).SingleOrDefault();
            var admin = zbor.Voditelj.OrderByDescending(z => z.DatumPostanka).First();
            var mod = zbor.ModeratorZbora.Where(m => m.IdKorisnik == idKorisnik).SingleOrDefault();
            return admin.IdKorisnik == idKorisnik || mod != null ? true : false;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var korisnik = _ctx.Korisnik.Where(k => k.Id == user.Id).SingleOrDefault();
            //ispravi
            var mojiZborovi = _ctx.Zbor.Where(z => z.Voditelj.Select(v => v.IdKorisnik).Contains(user.Id) || z.ClanZbora.Select(v => v.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var prijaveZborovi = _ctx.Zbor.Where(z => z.PrijavaZaZbor.Select(p => p.IdKorisnik).Contains(user.Id)).Include(p => p.PrijavaZaZbor).AsEnumerable();
            var ostaliZborovi =  _ctx.Zbor.Where(z => !z.Voditelj.Select(v => v.IdKorisnik).Contains(user.Id) && !z.ClanZbora.Select(v => v.IdKorisnik).Contains(user.Id) && !z.PrijavaZaZbor.Select(p => p.IdKorisnik).Contains(user.Id) && !z.PozivZaZbor.Select(p => p.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var mojiPozivi = _ctx.PozivZaZbor.Where(p => p.IdKorisnik == korisnik.Id).Include(p => p.IdZborNavigation).OrderByDescending(p => p.DatumPoziva).AsEnumerable();

            IndexViewModel model = new IndexViewModel { MojiPozivi=mojiPozivi,MojiZborovi = mojiZborovi, PoslanePrijaveZborovi = prijaveZborovi, OstaliZborovi = ostaliZborovi, KorisnikId= user.Id };
            return View(model);
        }

        [HttpGet]
        public IActionResult Dodaj()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dodaj(DodajViewModel model)
        {
            if(ModelState.IsValid)
            {
                if(model.Novi.Naziv.Trim().Equals(""))
                {
                    ModelState.AddModelError("Naziv", "Naziv je obavezan.");
                }
                if(model.Novi.Adresa.Trim().Equals(""))
                {
                    ModelState.AddModelError("Adresa", "Adresa je obavezna");
                }
                if(!ModelState.IsValid)
                {
                    return View(model);
                }
                model.Novi.Id = Guid.NewGuid();
                ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
                Voditelj voditelj = new Voditelj
                {
                    DatumPostanka = DateTime.Now,
                    Id = Guid.NewGuid(),
                    IdZborNavigation = model.Novi,
                    IdZbor = model.Novi.Id,
                    IdKorisnik = user.Id
                };
                ClanZbora clan = new ClanZbora
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = user.Id,
                    IdZbor = model.Novi.Id,
                    DatumPridruzivanja = DateTime.Now,
                    Glas = "ne"
                };
                model.Novi.Voditelj.Add(voditelj);
                model.Novi.ClanZbora.Add(clan);
                _ctx.Add(model.Novi);
                _ctx.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
       
     

        [HttpGet]
        public async Task<IActionResult> Profil(Guid id)
        {
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
            var korisnik = _ctx.Korisnik.Find(user.Id);
            Zbor zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.Voditelj).Include(z => z.Projekt).SingleOrDefault();
            IEnumerable<Obavijest> obavijesti = _ctx.Obavijest.Where(o => o.IdZbor == id)
                .Include(o => o.IdKorisnikNavigation)
                .Include(o => o.LajkObavijesti)
                .Include(o => o.KomentarObavijesti).ThenInclude(k => k.LajkKomentara)
                .Include(o => o.KomentarObavijesti).ThenInclude(k => k.IdKorisnikNavigation)
                  .Include(o => o.KomentarObavijesti).OrderBy(d => d.DatumObjave)
                .OrderByDescending(O => O.DatumObjave);
            ProfilViewModel model = new ProfilViewModel { Zbor = zbor, Obavijesti = obavijesti, IdKorisnik = user.Id, ImeIPrezime = korisnik.Ime + " " + korisnik.Prezime, Slika = korisnik.IdSlika };
            var admin = zbor.Voditelj.OrderByDescending(z => z.DatumPostanka).First();
            model.Admin = IsAdmin(id, user.Id);
            model.Projekti = zbor.Projekt.Where(p => !p.Zavrsen).ToList();
            ViewData["zborId"] = id;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            ViewData["Model"] = TempData["Model"];
            return View(model);
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profil(Guid id, ProfilViewModel model)
        {
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
            if (ModelState.IsValid)
            {
                if (model.NovaObavijest.Naslov.Trim().Equals(""))
                {
                    ModelState.AddModelError("Naslov", "Naslov je obavezan.");
                }
                if (model.NovaObavijest.Tekst.Trim().Equals(""))
                {
                    ModelState.AddModelError("Adresa", "Tekst je obavezan");
                }
                if (!ModelState.IsValid)
                {
                    TempData["Model"] = "Ispravite greške unutar polja forme.";
                    return RedirectToAction("Profil", new { id = id });
                }


                model.Zbor = _ctx.Zbor.Find(id);
                model.NovaObavijest.DatumObjave = DateTime.Now;
                model.NovaObavijest.Id = Guid.NewGuid();
                model.NovaObavijest.IdZbor = id;
                model.NovaObavijest.IdKorisnik = user.Id;

                var pretplatnici = _ctx.PretplataNaZbor.Where(p => p.IdZbor == id && p.Obavijesti).Select(p => p.IdKorisnik).ToHashSet();

                if (model.OdabraniProjekti != null)
                {
                    var projekti = model.OdabraniProjekti.Split(",");
                    foreach(var projektId in projekti)
                    {
                        var idProj = Guid.Parse(projektId);
                        var pro = _ctx.Projekt.Find(idProj);
                        if(pro == null || pro.IdZbor != id)
                        {
                            continue;
                        }
                        var ob = new ObavijestVezanaUzProjekt
                        {
                            Id = Guid.NewGuid(),
                            IdProjekt = Guid.Parse(projektId),
                            IdObavijest = model.NovaObavijest.Id
                        };
                        model.NovaObavijest.ObavijestVezanaUzProjekt.Add(ob);
                        pretplatnici.Add(_ctx.PretplataNaProjekt.Where(p => p.IdProjekt == idProj && p.Obavijesti).Select(p => p.IdKorisnik).SingleOrDefault());
                    }
                }
                _ctx.Add(model.NovaObavijest);
                _ctx.SaveChanges();
                foreach(var pret in pretplatnici)
                {
                    OsobneObavijesti ob = new OsobneObavijesti
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = pret,
                        Tekst = "Nova obavijest u zboru <b>" + model.Zbor.Naziv + ".</b>",
                        Procitano = false,
                        Poveznica = "/Zbor/Profil/" + model.Zbor.Id
                    };
                    _ctx.Add(ob);
                    await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
                 

                }
                _ctx.SaveChanges();
                return RedirectToAction("Profil", new { id = id });
            }
            TempData["Model"] = "Ispravite greške unutar polja forme.";
            return RedirectToAction("Profil", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> LajkObavijesti([FromBody] LajkModel lajk)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idObavijest;
            var flag = Guid.TryParse(lajk.IdCilj, out idObavijest);
            if (flag == false)
                return BadRequest();

            var obavijest = _ctx.Obavijest.Find(idObavijest);
            if (obavijest == null)
                return NoContent();
            if (!CheckRights(obavijest.IdZbor, user.Id))
                return Forbid();
            var lajkPostoji = _ctx.LajkObavijesti.Where(l => l.IdObavijest == idObavijest && l.IdKorisnik == user.Id).SingleOrDefault();
            if (lajkPostoji != null)
            {
                return Ok();
            }
            var l = new LajkObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdObavijest = idObavijest

            };
            var brojLajkova = _ctx.LajkObavijesti.Where(l => l.IdObavijest == idObavijest).Count();
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = obavijest.IdKorisnik,
                Tekst = String.Format("<b>{0}</b> označava tvoju objavu sa sviđa mi se.",_ctx.Korisnik.Find(user.Id).ImeIPrezime()),
                Procitano = false,
                Poveznica = "/Zbor/Profil/" + obavijest.IdZbor
            };

            _ctx.Add(ob);
            await _hubContext.Clients.User(obavijest.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == obavijest.IdZbor ).AsEnumerable())
            {
                await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkObavijesti", new { id = idObavijest, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk=true, brojLajkova=brojLajkova+1 }); ;
            }
            _ctx.LajkObavijesti.Add(l);
            _ctx.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UnlajkObavijesti([FromBody] LajkModel lajk)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idObavijest;
            var flag = Guid.TryParse(lajk.IdCilj, out idObavijest);
            if (flag == false)
                return BadRequest();
            var obavijest = _ctx.Obavijest.Find(idObavijest);
            if (obavijest == null)
                return NoContent();
            if (!CheckRights(obavijest.IdZbor, user.Id))
                return Forbid();

            var l = _ctx.LajkObavijesti.Where(l => l.IdKorisnik == user.Id && l.IdObavijest == Guid.Parse(lajk.IdCilj)).SingleOrDefault();
            if (l != null)
            {
                var brojLajkova = _ctx.LajkObavijesti.Where(l => l.IdObavijest == idObavijest).Count();
                foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == obavijest.IdZbor).AsEnumerable())
                {
                    await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkObavijesti", new { id = idObavijest, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk = false, brojLajkova=brojLajkova-1 }); ;
                }
                _ctx.LajkObavijesti.Remove(l);
                _ctx.SaveChanges();
            }
            return Ok();
            
        }

        [HttpPost]
        public async Task<IActionResult> LajkKomentara([FromBody] LajkModel lajk)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idKomentar;
            var flag = Guid.TryParse(lajk.IdCilj, out idKomentar);
            if (flag == false)
                return BadRequest();

            var komentar = _ctx.KomentarObavijesti.Where(k => k.Id == idKomentar).Include(k => k.IdObavijestNavigation).SingleOrDefault();
            if (komentar == null)
                return NoContent();
            if (!CheckRights(komentar.IdObavijestNavigation.IdZbor, user.Id))
                return Forbid();
            var lajkPostoji = _ctx.LajkKomentara.Where(l => l.IdKomentar == idKomentar && l.IdKorisnik == user.Id).SingleOrDefault();
            if (lajkPostoji != null)
            {
                return Ok();
            }

            var l = new LajkKomentara
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdKomentar = idKomentar

            };
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = komentar.IdKorisnik,
                Tekst = String.Format("<b>{0}</b> označava tvoj komentar sa sviđa mi se.", _ctx.Korisnik.Find(user.Id).ImeIPrezime()),
                Procitano = false,
                Poveznica = "/Zbor/Profil/" + komentar.IdObavijestNavigation.IdZbor
            };

            _ctx.Add(ob);
            await _hubContext.Clients.User(komentar.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            var brojLajkova = _ctx.LajkKomentara.Where(l => l.IdKomentar == idKomentar).Count();
            foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == komentar.IdObavijestNavigation.IdZbor).AsEnumerable())
            {
                await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkKomentara", new { id = idKomentar, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk = true, brojLajkova = brojLajkova + 1 }); ;
            }
            _ctx.LajkKomentara.Add(l);
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> UnlajkKomentara([FromBody] LajkModel lajk)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idKomentar;
            var flag = Guid.TryParse(lajk.IdCilj, out idKomentar);
            if (flag == false)
                return BadRequest();

            var komentar = _ctx.KomentarObavijesti.Where(k => k.Id == idKomentar).Include(k => k.IdObavijestNavigation).SingleOrDefault();
            if (komentar == null)
                return NoContent();
            if (!CheckRights(komentar.IdObavijestNavigation.IdZbor, user.Id))
                return Forbid();
            var l = _ctx.LajkKomentara.Where(l => l.IdKorisnik == user.Id && l.IdKomentar == idKomentar).SingleOrDefault();
            if(l!= null)
            {
                var brojLajkova = _ctx.LajkKomentara.Where(l => l.IdKomentar == idKomentar).Count();
                foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == komentar.IdObavijestNavigation.IdZbor).AsEnumerable())
                {
                    await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkKomentara", new { id = idKomentar, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk = false, brojLajkova = brojLajkova - 1 }); ;
                }
                _ctx.LajkKomentara.Remove(l);
                _ctx.SaveChanges();
            }
            
            return Ok();

        }
        [HttpPost]
        public async Task<IActionResult> NoviKomentar([FromBody] NoviKomentarModel komentar)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idObavijest;
            var flag = Guid.TryParse(komentar.IdObavijest, out idObavijest);
            if (flag == false)
                return BadRequest();

            var obavijest = _ctx.Obavijest.Find(idObavijest);
            if (obavijest == null)
                return NoContent();
            if (!CheckRights(obavijest.IdZbor, user.Id))
                return Forbid();

            var k = new KomentarObavijesti
            {
                DatumObjave = DateTime.Now,
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdObavijest = idObavijest,
                Tekst = komentar.Tekst.Trim()
            };
            var korisnik = _ctx.Korisnik.Find(user.Id);
            var kom = new
            {
                Datum = k.DatumObjave.ToString("dd.MM.yyyy. HH:mm"),
                Id = k.Id,
                IdKorisnik = k.IdKorisnik,
                IdObavijest = k.IdObavijest,
                Tekst = k.Tekst,
                ImeIPrezime = korisnik.ImeIPrezime(),
                Slika = korisnik.IdSlika,
            };
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = obavijest.IdKorisnik,
                Tekst = String.Format("<b>{0}</b> komentira tvoju.", korisnik.ImeIPrezime()),
                Procitano = false,
                Poveznica = "/Zbor/Profil/" + obavijest.IdZbor
            };

            _ctx.Add(ob);
            await _hubContext.Clients.User(obavijest.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == obavijest.IdZbor).AsEnumerable())
            {
                await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("NoviKomentar", kom); 
            }
            _ctx.KomentarObavijesti.Add(k);
            _ctx.SaveChanges();
            return Ok(new { id = "kok" });
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiKomentar([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idKomentar;
            var flag = Guid.TryParse(model.Value, out idKomentar);
            if (flag == false)
                return BadRequest();

            var komentar = _ctx.KomentarObavijesti.Where(k => k.Id == idKomentar).Include(k => k.IdObavijestNavigation).SingleOrDefault();
            if (komentar == null)
                return NoContent();
            if (komentar.IdKorisnik != user.Id && !IsAdmin(komentar.IdObavijestNavigation.IdZbor, user.Id))
                return Forbid();
            foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == komentar.IdObavijestNavigation.IdZbor).AsEnumerable())
            {
                await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("ObrisiKomentar", new { id = idKomentar }); ;
            }
            _ctx.KomentarObavijesti.Remove(komentar);
            _ctx.SaveChanges();
            return Ok("32");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiObavijest(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var obavijest = _ctx.Obavijest.Find(id);
            if (obavijest == null)
                return RedirectToAction("Nema", "Greska");
            if (obavijest.IdKorisnik != user.Id && !IsAdmin(obavijest.IdZbor, user.Id))
                return RedirectToAction("Prava");

            Obavijest o = _ctx.Obavijest.Find(id);
            _ctx.Obavijest.Remove(o);
            _ctx.SaveChanges();

            return RedirectToAction("Profil", new { id = obavijest.IdZbor });
        }

        
        [HttpGet]
        public async Task<IActionResult> Pitanja(Guid id)
        {
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");

            Zbor zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.Voditelj).Include(z => z.Projekt).SingleOrDefault();
            var aktivna = _ctx.Anketa.Where(a => a.IdZbor == id && a.DatumKraja >= DateTime.Now).Include(a => a.IdKorisnikNavigation).Include(a => a.OdgovorAnkete).ThenInclude(o => o.OdgovorKorisnikaNaAnketu).OrderBy(a => a.DatumKraja);
            var prosla = _ctx.Anketa.Where(a => a.IdZbor == id && a.DatumKraja < DateTime.Now).Include(a => a.IdKorisnikNavigation).Include(a => a.OdgovorAnkete).ThenInclude(o => o.OdgovorKorisnikaNaAnketu).OrderByDescending(a => a.DatumKraja);
            Dictionary<Guid, List<int>> korisnickiOdgovori = new Dictionary<Guid, List<int>>();
            foreach (var anketa in aktivna)
            {
                List<int> odg = new List<int>();
                foreach (var odgovor in anketa.OdgovorAnkete)
                {
                    OdgovorKorisnikaNaAnketu odgovorNaPitanje = odgovor.OdgovorKorisnikaNaAnketu.Where(o => o.IdOdgovor == odgovor.Id && o.IdKorisnik == user.Id).FirstOrDefault();
                    if (odgovorNaPitanje != null)
                        odg.Add(odgovor.Redoslijed);
                }
                korisnickiOdgovori.Add(anketa.Id, odg);
            }
            foreach (var anketa in prosla)
            {
                List<int> odg = new List<int>();
                foreach (var odgovor in anketa.OdgovorAnkete)
                {
                    OdgovorKorisnikaNaAnketu odgovorNaPitanje = odgovor.OdgovorKorisnikaNaAnketu.Where(o => o.IdOdgovor == odgovor.Id && o.IdKorisnik == user.Id).FirstOrDefault();
                    if (odgovorNaPitanje != null)
                        odg.Add(odgovor.Redoslijed);
                }
                korisnickiOdgovori.Add(anketa.Id, odg);
            }
            PitanjaViewModel model = new PitanjaViewModel
            {
                Admin = IsAdmin(id, user.Id),
                AktivnaPitanja = aktivna,
                GotovaPitanja = prosla,
                KorisnickiOdgovori = korisnickiOdgovori,
                IdZbor = id,
                IdKorisnik = user.Id
            };
            ViewData["zborId"] = id;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> NovaAnketa(Guid id)
        {
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
            var zbor = _ctx.Zbor.Find(id);
           
            ViewData["zborId"] = id;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            ViewData["Model"] = TempData["Model"];
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovaAnketa(Guid id, AnketaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            PitanjeJSON anketaJSON = JsonConvert.DeserializeObject<PitanjeJSON>(model.Json);
            Anketa anketa = new Anketa
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdZbor = id,
                DatumPostavljanja = DateTime.Now,
                Pitanje = anketaJSON.pitanje.Trim(),
                DatumKraja = model.DatumKraj,
                VisestrukiOdgovor = anketaJSON.vrsta.Equals("one") ? false : true
                
            };
            if(anketa.Pitanje.Equals("") || !ModelState.IsValid || anketaJSON.odgovori.Count == 0)
            {
                TempData["Model"] = "Ispravno zadajte pitanje";
            }
            int i = 0;
            foreach(string odgovor in anketaJSON.odgovori)
            {
                OdgovorAnkete odg = new OdgovorAnkete
                {
                    Id = Guid.NewGuid(),
                    IdAnketa = anketa.Id,
                    Redoslijed = i,
                    Odgovor = odgovor.Trim(),
                    
                };
                if (odg.Odgovor.Equals(""))
                {
                    TempData["Model"] = "Ispravno zadajte pitanje";
                }
                anketa.OdgovorAnkete.Add(odg);
                i++;
            }
            if(TempData["Model"] != null)
            {
                return RedirectToAction("NovaAnketa", new { id = id });
            }
            _ctx.Anketa.Add(anketa);
            var pretplatnici = _ctx.PretplataNaZbor.Where(p => p.IdZbor == id && p.Pitanja).Select(p => p.IdKorisnik).ToHashSet();
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret,
                    Tekst = "Novo pitanje u zboru <b>" + _ctx.Zbor.Find(id).Naziv + ".</b>",
                    Procitano = false,
                    Poveznica = "/Zbor/Pitanja/" + id
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }

            _ctx.SaveChanges();
            return RedirectToAction("Pitanja", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> OdgovoriNaPitanje([FromBody] ListaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idAnketa;
            var flag = Guid.TryParse(model.Id, out idAnketa);
            if (flag == false)
                return BadRequest();

            var pitanje = _ctx.Anketa.Find(idAnketa);
            if (pitanje == null)
                return NoContent();
            if (!CheckRights(pitanje.IdZbor, user.Id))
                return Forbid();

            var stariOdgovor = _ctx.OdgovorKorisnikaNaAnketu.Where(o => o.IdOdgovorNavigation.IdAnketa == idAnketa && o.IdKorisnik == user.Id).ToList();
            if (stariOdgovor != null)
                _ctx.OdgovorKorisnikaNaAnketu.RemoveRange(stariOdgovor);
            foreach (var odg in model.Lista)
            {
                try
                {
                    OdgovorKorisnikaNaAnketu odgovor = new OdgovorKorisnikaNaAnketu
                    {
                        DatumOdgovora = DateTime.Now,
                        Id = Guid.NewGuid(),
                        IdKorisnik = user.Id
                    };
                    odgovor.IdOdgovor = _ctx.OdgovorAnkete.Where(o => o.IdAnketa == idAnketa && o.Redoslijed == Int32.Parse(odg)).SingleOrDefault().Id;
                    _ctx.OdgovorKorisnikaNaAnketu.Add(odgovor);
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiPitanje(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var pitanje = _ctx.Anketa.Find(id);
            if (pitanje == null)
                return RedirectToAction("Nema", "Greska");
            if (pitanje.IdKorisnik != user.Id && !IsAdmin(pitanje.IdZbor, user.Id))
                return RedirectToAction("Prava");

            Anketa p = _ctx.Anketa.Find(id);
            _ctx.Anketa.Remove(p);
            _ctx.SaveChanges();

            return RedirectToAction("Pitanja", new { id = pitanje.IdZbor });
        }
        [HttpGet]
        public async Task<IActionResult> Administracija(Guid id)
        {
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!IsAdmin(id, user.Id))
                return RedirectToAction("Prava");
            ViewData["mess"] = TempData["mess"];
            AdministracijaViewModel model = new AdministracijaViewModel();
            var zbor = _ctx.Zbor.Where(z => z.Id == id)
                .Include(z => z.PozivZaZbor).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.PrijavaZaZbor).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.ClanZbora).ThenInclude(c => c.IdKorisnikNavigation)
                .Include(z => z.ModeratorZbora).ThenInclude(c => c.IdKorisnikNavigation)
                .Include(z => z.Voditelj).ThenInclude(c => c.IdKorisnikNavigation)
                .SingleOrDefault();
           
            model.Zbor = zbor;
            model.Soprani = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("sopran")).ToList();
            model.Alti = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("alt")).ToList();
            model.Tenori = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("tenor")).ToList();
            model.Basi = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("bas")).ToList();
            model.Nerazvrstani = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("ne")).ToList();
            model.Voditelj = zbor.Voditelj.OrderByDescending(z => z.DatumPostanka).First().IdKorisnikNavigation;
            model.Vod = model.Voditelj.Id == user.Id;
            ViewData["zborId"] = id;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            ViewData["mess"] = TempData["mess"];
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> PrijavaZaZbor([FromBody] PrijavaModel prijava)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idZbor;
            var flag = Guid.TryParse(prijava.Id, out idZbor);
            if (flag == false)
                return BadRequest();
            if (!Exists(idZbor))
                return NotFound();
            var prijavaZaZbor = _ctx.PrijavaZaZbor.Where(p => p.IdKorisnik == user.Id && p.IdZbor == idZbor).SingleOrDefault();
            if (prijavaZaZbor != null)
                return Ok();
            var clan = _ctx.ClanZbora.Where(p => p.IdKorisnik == user.Id && p.IdZbor == idZbor).SingleOrDefault();
            if (clan != null)
                return Ok();
            var pr = new PrijavaZaZbor
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdZbor = idZbor,
                Poruka = prijava.Poruka.Trim(),
                DatumPrijave = DateTime.Now
            };
            _ctx.PrijavaZaZbor.Add(pr);
            var pretplatnici = _ctx.ModeratorZbora.Where(p => p.IdZbor == idZbor).Select(p => p.IdKorisnik).ToHashSet();
            pretplatnici.Add(_ctx.Voditelj.Where(v => v.IdZbor == idZbor).Select(p => p.IdKorisnik).FirstOrDefault());
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret,
                    Tekst = String.Format("<b>{0}</b> se prijavljuje za zbor <b>{1}</b>.",_ctx.Korisnik.Find(user.Id).ImeIPrezime() ,_ctx.Zbor.Find(idZbor).Naziv),
                    Procitano = false,
                    Poveznica = "/Zbor/Administracija/" + idZbor
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }


            _ctx.SaveChanges();
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }
        [HttpPost]
        public async Task<IActionResult> PrihvatiPrijavu([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idPrijava;
            var flag = Guid.TryParse(model.Value, out idPrijava);
            if (flag == false)
                return BadRequest();
            PrijavaZaZbor prijava = _ctx.PrijavaZaZbor.Where(p => p.Id == idPrijava).Include(p => p.IdKorisnikNavigation).SingleOrDefault();
            if (!IsAdmin(prijava.IdZbor, user.Id))
                return Forbid();
            if (prijava == null)
                return Ok();
            var cl = _ctx.ClanZbora.Where(p => p.IdKorisnik == prijava.IdKorisnik && p.IdZbor == prijava.IdZbor).SingleOrDefault();
            if (cl != null)
                return Ok();
            ClanZbora clan = new ClanZbora
            {
                Id = Guid.NewGuid(),
                DatumPridruzivanja = DateTime.Now,
                Glas = "ne",
                IdZbor = prijava.IdZbor,
                IdKorisnik = prijava.IdKorisnik
            };
            PretplataNaZbor pret = new PretplataNaZbor
            {
                Id = Guid.NewGuid(),
                IdKorisnik = prijava.IdKorisnik,
                IdZbor = prijava.IdZbor,
                Obavijesti = true,
                Pitanja = true,
                Repozitorij = true
            };
            _ctx.ClanZbora.Add(clan);
            _ctx.PretplataNaZbor.Add(pret);
            _ctx.PrijavaZaZbor.Remove(prijava);
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = prijava.IdKorisnik,
                Tekst = String.Format("Prihvaćena prijava za zbor <b>{0}</b>.",  _ctx.Zbor.Find(prijava.IdZbor).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/Profil/" + prijava.IdZbor
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(prijava.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            return Ok(new { ImeIPrezime = prijava.IdKorisnikNavigation.ImeIPrezime(), id=clan.Id, idZbor = clan.IdZbor });
        }
        [HttpPost]
        public async Task<IActionResult> PromjenaGlasa([FromBody] PrijavaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idClan;
            var flag = Guid.TryParse(model.Id, out idClan);
            if (flag == false)
                return BadRequest();
            var clan = _ctx.ClanZbora.Find(idClan);
            if (clan == null)
                return NotFound();
            if (!IsAdmin(clan.IdZbor, user.Id))
                return Forbid();

            string glas = "";
            if (model.Poruka.Equals("1")) glas = "sopran";
            else if (model.Poruka.Equals("2")) glas = "alt";
            else if (model.Poruka.Equals("3")) glas = "tenor";
            else if (model.Poruka.Equals("4")) glas = "bas";
            clan.Glas = glas;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }
        [HttpPost]
        public async Task<IActionResult> PromjenaUloge([FromBody] PrijavaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idClan;
            var flag = Guid.TryParse(model.Id, out idClan);
            if (flag == false)
                return BadRequest();
            var clan = _ctx.ClanNaProjektu.Where(c => c.Id == idClan).Include(c => c.IdProjektNavigation).SingleOrDefault();
            if (clan == null)
                return NotFound();
            if (!IsAdmin(clan.IdProjektNavigation.IdZbor, user.Id))
                return Forbid();
            if (clan.IdProjektNavigation.Zavrsen)
                return Conflict();
            var id = Guid.Parse(model.Id);
            string glas = model.Poruka.Trim();
            clan.Uloga = glas;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }

        [HttpPost]
        public async Task<IActionResult> OdbijPrijavu([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idPrijava;
            var flag = Guid.TryParse(model.Value, out idPrijava);
            if (flag == false)
                return BadRequest();
            var prijava = _ctx.PrijavaZaZbor.Find(idPrijava);
            if (prijava == null)
                return NotFound();
            if (!IsAdmin(prijava.IdZbor, user.Id) || user.Id == prijava.Id)
                return Forbid();
            _ctx.PrijavaZaZbor.Remove(prijava);
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = prijava.IdKorisnik,
                Tekst = String.Format("Odbijena prijava za zbor <b>{0}</b>.", _ctx.Zbor.Find(prijava.IdZbor).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/JavniProfil/" + prijava.IdZbor
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(prijava.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }
        [HttpPost]
        public async Task<IActionResult> NoviModerator([FromBody] LajkModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idZbor, idMod;
            var flagZb = Guid.TryParse(model.IdCilj, out idZbor);
            if (flagZb == false)
                return BadRequest();
            var flagK = Guid.TryParse(model.IdKorisnik, out idMod);
            if (flagK == false)
                return BadRequest();
            if (!Exists(idZbor) || _ctx.Korisnik.Find(idMod) == null)
                return NotFound();
            if (!IsAdmin(idZbor, user.Id))
                return Forbid();
            var clan = _ctx.ClanZbora.Where(c => c.IdZbor == idZbor && c.IdKorisnik == idMod).SingleOrDefault();
            if (clan == null)
                return NotFound();
            
            ModeratorZbora mod = new ModeratorZbora
            {
                Id = Guid.NewGuid(),
                IdKorisnik = idMod,
                IdZbor = idZbor

            };
            _ctx.ModeratorZbora.Add(mod);
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = idMod,
                Tekst = String.Format("Postali ste moderator zbora <b>{0}</b>.", _ctx.Zbor.Find(idZbor).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/Administracija/" + idZbor,
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(idMod.ToString()).SendAsync("NovaObavijest", new {id=ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            return Ok(new {id=mod.Id, ImeIPrezime= _ctx.Korisnik.Find(Guid.Parse(model.IdKorisnik)).ImeIPrezime() });
        }
        [HttpPost]
        public async  Task<IActionResult> ObrisiModeratora([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idMod;
            var flag = Guid.TryParse(model.Value, out idMod);
            if (flag == false)
                return BadRequest();
            ModeratorZbora mod = _ctx.ModeratorZbora.Find(idMod);
            if (mod == null)
                return NotFound();
            if (!IsAdmin(mod.IdZbor, user.Id))
                return Forbid();
            _ctx.ModeratorZbora.Remove(mod);
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = mod.IdKorisnik,
                Tekst = String.Format("Više niste moderator zbora <b>{0}</b>.", _ctx.Zbor.Find(mod.IdZbor).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/Profil/" + mod.IdZbor
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(mod.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            if(idMod==user.Id)
            {
                return RedirectToAction("Administracija", new { id = mod.IdZbor });
            }
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostaviVoditelja(AdministracijaViewModel modell)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var clan = _ctx.ClanZbora.Find(modell.IdBrisanje);
            if (clan == null)
                return RedirectToAction("Nema", "Greska");
            var vod = _ctx.Voditelj.Where(v => v.IdKorisnik == user.Id && v.IdZbor ==clan.IdZbor).SingleOrDefault();
            if(vod == null)
            {
                TempData["mess"] = "Samo voditelj može postaviti novog voditelja";
                return RedirectToAction("Administracija", new { id = clan.IdZbor });
            }
            _ctx.Voditelj.Remove(vod);
            Voditelj v = new Voditelj
            {
                Id = Guid.NewGuid(),
                IdKorisnik = clan.IdKorisnik,
                IdZbor = clan.IdZbor

            };
            _ctx.Voditelj.Add(v);
            var clanovi = _ctx.ClanZbora.Where(c => c.IdZbor == clan.IdZbor).ToList();
            foreach(var cl in clanovi)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = cl.IdKorisnik,
                    Tekst = String.Format("<b>{0}</b> je novi voditelj zbora <b>{1}</b>.", _ctx.Korisnik.Find(clan.IdKorisnik).ImeIPrezime(), _ctx.Zbor.Find(clan.IdZbor).Naziv),
                    Procitano = false,
                    Poveznica = "/Zbor/Profil/" + cl.IdZbor
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(cl.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            }
            _ctx.SaveChanges();
            return RedirectToAction("Profil", new { id = clan.IdZbor });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiClana(AdministracijaViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var clan = _ctx.ClanZbora.Find(model.IdBrisanje);
            if (clan == null)
                return RedirectToAction("Nema", "Greska");
            if (clan.IdKorisnik == user.Id)
                return RedirectToAction("Prava");
            if (!IsAdmin(clan.IdZbor, user.Id))
                return RedirectToAction("Prava");
            _ctx.ClanZbora.Remove(clan);
            var pretplata = _ctx.PretplataNaZbor.Where(p => p.IdKorisnik == clan.IdKorisnik && p.IdZbor == clan.IdZbor).ToList();
            _ctx.RemoveRange(pretplata);
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = clan.IdKorisnik,
                Tekst = String.Format("Izbačeni ste iz zbora <b>{0}</b>.",  _ctx.Zbor.Find(clan.IdZbor).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/JavniProfil/" + clan.IdZbor
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            return RedirectToAction("Administracija", new { id = clan.IdZbor });
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiClanaProjekta(AdministracijaProjektaViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var clan = _ctx.ClanNaProjektu.Where(c => c.Id == model.IdBrisanje).Include(c => c.IdProjektNavigation).SingleOrDefault();
            if (clan == null)
                return RedirectToAction("Nema", "Greska");
            if (!IsAdmin(clan.IdProjektNavigation.IdZbor, user.Id))
                return RedirectToAction("Prava");
            if (clan.IdProjektNavigation.Zavrsen)
                return RedirectToAction("Nema", "Greska");
            _ctx.ClanNaProjektu.Remove(clan);
            var pretplata = _ctx.PretplataNaProjekt.Where(p => p.IdKorisnik == clan.IdKorisnik && p.IdProjekt == clan.IdProjekt).ToList();
            _ctx.RemoveRange(pretplata);
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = clan.IdKorisnik,
                Tekst = String.Format("Više niste na projektu <b>{0}</b>.", _ctx.Projekt.Find(clan.IdProjekt).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/Projekt/" + clan.IdProjekt
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            return RedirectToAction("AdministracijaProjekta", new { id = clan.IdProjekt });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiProjekt(AdministracijaProjektaViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var projekt = _ctx.Projekt.Find(model.IdBrisanje);
            if(projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            _ctx.Remove(projekt);
            _ctx.SaveChanges();

            return RedirectToAction("Projekti", new { id = projekt.IdZbor });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ZavrsiProjekt(AdministracijaProjektaViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var projekt = _ctx.Projekt.Find(model.IdBrisanje);
            if (projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            projekt.Zavrsen = true;
            _ctx.SaveChanges();

            return RedirectToAction("Projekti", new { id = projekt.IdZbor });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OtvoriProjekt(AdministracijaProjektaViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var projekt = _ctx.Projekt.Find(model.IdBrisanje);
            if (projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            projekt.Zavrsen = false;
            _ctx.SaveChanges();

            return RedirectToAction("Projekti", new { id = projekt.IdZbor });
        }




        [HttpPost]
        public async Task<IActionResult> ObrisiPoziv([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idPoziv;
            var flag = Guid.TryParse(model.Value, out idPoziv);
            if (flag == false)
                return BadRequest();
            PozivZaZbor poz = _ctx.PozivZaZbor.Find(idPoziv);
            if (poz == null)
                return NotFound();
            if (!IsAdmin(poz.IdZbor, user.Id))
                return Forbid();
            _ctx.PozivZaZbor.Remove(poz);
            _ctx.SaveChanges();
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }


        [HttpPost]
        public IActionResult PretragaKorisnika([FromBody] PretragaModel model)
        {
            Guid idZbor = Guid.Parse(model.Id);
            var korisnici = _ctx.Korisnik.Where(k => (k.Ime.Trim().ToLower() + ' ' + k.Prezime.Trim().ToLower()).Contains(model.Tekst))
                .Where(k => k.ClanZbora.Where(c => c.IdZbor == idZbor && c.IdKorisnik == k.Id).SingleOrDefault() == null && k.PozivZaZbor.Where(c => c.IdZbor == idZbor && c.IdKorisnik == k.Id).SingleOrDefault() == null && k.PrijavaZaZbor.Where(c => c.IdZbor == idZbor && c.IdKorisnik == k.Id).SingleOrDefault() == null)
                .Select(k => new {Id = k.Id, ImeIPrezime = k.Ime.Trim() + ' ' + k.Prezime.Trim() })
                .ToList();
            
            return Ok(korisnici);
        }

        [HttpGet]
        public async Task<IActionResult> Projekti(Guid id)
        {
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
           
            var mojiProjekti = _ctx.Projekt.Where(p => id==p.IdZbor && !p.Zavrsen).Where(z => z.ClanNaProjektu.Select(v => v.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var prijaveProjekti = _ctx.Projekt.Where(p => id == p.IdZbor && !p.Zavrsen).Where(z => z.PrijavaZaProjekt.Select(p => p.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var ostaliProjekti = _ctx.Projekt.Where(p => id == p.IdZbor && !p.Zavrsen).Where(z => !z.ClanNaProjektu.Select(v => v.IdKorisnik).Contains(user.Id) && !z.PrijavaZaProjekt.Select(p => p.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var zavrseniProjekti = _ctx.Projekt.Where(p => id == p.IdZbor && p.Zavrsen).ToList();
            ProjektiViewModel model = new ProjektiViewModel
            {
                IdZbor = id,
                MojiProjekti = mojiProjekti,
                IdKorisnik = user.Id,
                Admin = IsAdmin(id, user.Id),
                OstaliProjekti = ostaliProjekti,
                PrijavaProjekti = prijaveProjekti,
                Projekti = _ctx.Projekt.Where(p => p.IdZbor == id).Include(p => p.ClanNaProjektu).Include(p => p.PrijavaZaProjekt),
                VrstePodjele = _ctx.VrstaPodjele.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Podjela }).ToList(),
                ZavrseniProjekti = zavrseniProjekti
            };
            var zbor = _ctx.Zbor.Find(id);
            ViewData["zborId"] = id;
            ViewData["zborIme"] =zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            ViewData["Model"] = TempData["Model"];
            return View(model);
        }
  

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Projekti(ProjektiViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var zbor = _ctx.Zbor.Find(model.Novi.IdZbor);
            if(zbor == null)
                return RedirectToAction("Nema", "Greska");
            if (!CheckRights(zbor.Id, user.Id))
                return RedirectToAction("Prava");

            if (model.Novi.Naziv.Trim().Equals(""))
                ModelState.AddModelError("Naziv", "Naziv je obavezan");
            if(model.Novi.Opis.Trim().Equals(""))
                ModelState.AddModelError("Opis", "Opis je obavezan");

            if (ModelState.IsValid)
            {
                model.Novi.Id = Guid.NewGuid();
                model.Novi.ClanNaProjektu.Add(new ClanNaProjektu { Id = Guid.NewGuid(), IdKorisnik = user.Id, IdProjekt = model.Novi.Id, Uloga = "Nema" });
                _ctx.Add(model.Novi);
                var clanovi = _ctx.ClanZbora.Where(c => c.IdZbor == zbor.Id).ToList();
                foreach (var cl in clanovi)
                {
                    OsobneObavijesti ob = new OsobneObavijesti
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = cl.IdKorisnik,
                        Tekst = String.Format("Dodan je projekt <b>{0}</b> u zbor <b>{1}</b>.", model.Novi.Naziv, zbor.Naziv),
                        Procitano = false,
                        Poveznica = "/Zbor/Projekti/" + cl.IdZbor
                    };
                    _ctx.Add(ob);
                    await _hubContext.Clients.User(cl.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
                }
                _ctx.SaveChanges();
                return RedirectToAction("Projekti", new { id = model.Novi.IdZbor });
            }
            TempData["Model"] = "Ispravno popunite podatke o projektu";
            return RedirectToAction("Projekti", new { id = model.Novi.IdZbor });


        }

        [HttpPost]
        public async Task<IActionResult> PrijavaZaProjekt([FromBody] PrijavaModel prijava)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idProjekt;
            var flag = Guid.TryParse(prijava.Id, out idProjekt);
            if (flag == false)
                return BadRequest();
            var projekt = _ctx.Projekt.Find(idProjekt);
            if (projekt == null)
                return NotFound();
            if (projekt.Zavrsen)
                return Conflict();
            var prijavaZaProjekt = _ctx.PrijavaZaProjekt.Where(p => p.IdKorisnik == user.Id && p.IdProjekt == idProjekt).SingleOrDefault();
            if (prijavaZaProjekt != null)
                return Ok();
            if (!CheckRights(_ctx.Projekt.Find(idProjekt).IdZbor, user.Id))
                return Forbid();

            if (prijavaZaProjekt != null)
                return Ok();
            var pr = new PrijavaZaProjekt
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdProjekt = idProjekt,
                Poruka = prijava.Poruka,
                DatumPrijave = DateTime.Now
            };
            _ctx.PrijavaZaProjekt.Add(pr);
            var pretplatnici = _ctx.ModeratorZbora.Where(p => p.IdZbor == projekt.IdZbor).Select(p => p.IdKorisnik).ToHashSet();
            pretplatnici.Add(_ctx.Voditelj.Where(v => v.IdZbor == projekt.IdZbor).Select(p => p.IdKorisnik).FirstOrDefault());
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret,
                    Tekst = String.Format("<b>{0}</b> se prijavljuje za projekt <b>{1}</b>.", _ctx.Korisnik.Find(user.Id).ImeIPrezime(), projekt.Naziv),
                    Procitano = false,
                    Poveznica = "/Zbor/AdministracijaProjekta/" + projekt.Id
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }

            _ctx.SaveChanges();
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }

        [HttpGet]
        public async Task<IActionResult> AdministracijaProjekta(Guid id)
        {

            AdministracijaProjektaViewModel model = new AdministracijaProjektaViewModel();
            var projekt = _ctx.Projekt.Where(z => z.Id == id)
                .Include(z => z.PozivZaProjekt).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.PrijavaZaProjekt).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.ClanNaProjektu).ThenInclude(c => c.IdKorisnikNavigation)
                .Include(z => z.IdVrstePodjeleNavigation)
                .SingleOrDefault();
            if(projekt == null)
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!IsAdmin(projekt.IdZbor, user.Id)) 
                return RedirectToAction("Prava");

            model.Projekt = projekt;
            foreach(var glas in projekt.IdVrstePodjeleNavigation.Glasovi())
            {
                model.Clanovi[glas] = new List<ClanNaProjektu>();
            }
            foreach(var clan in projekt.ClanNaProjektu)
            {
                if (!clan.Uloga.Equals("Nema"))
                {
                   
                    model.Clanovi[clan.Uloga].Add(clan);
                }
                else
                    model.Nerazvrstani.Add(clan);
            }
            var zbor = _ctx.Zbor.Find(projekt.IdZbor);
            ViewData["zborId"] = projekt.IdZbor;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Dogadjaj(Guid id)
        {
            var dog = _ctx.Dogadjaj.Where(d => d.Id == id)
                .Include(d => d.NajavaDolaska).ThenInclude(n => n.IdKorisnikNavigation)
                .Include(d => d.IdProjektNavigation).ThenInclude(p => p.IdVrstePodjeleNavigation).SingleOrDefault();
            if (dog == null)
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(dog.IdProjektNavigation.IdZbor, user.Id))
                return RedirectToAction("Prava");
            dog.IdProjekt1 = _ctx.VrstaDogadjaja.Find(dog.IdVrsteDogadjaja);
            DogadjajViewModel model = new DogadjajViewModel();
            model.Dogadjaj = dog;
            model.isAdmin = IsAdmin(dog.IdProjektNavigation.IdZbor, user.Id);
            foreach (var glas in dog.IdProjektNavigation.IdVrstePodjeleNavigation.Glasovi())
            {
                model.Clanovi[glas] = new List<NajavaDolaska>();
                model.ClanoviProjekta[glas] = new List<ClanNaProjektu>();

            }
            foreach (var najava in dog.NajavaDolaska)
            {
                var clan = _ctx.ClanNaProjektu.Where(c => c.IdKorisnik == najava.IdKorisnik && c.IdProjekt == dog.IdProjekt).SingleOrDefault();
                if (clan == null) continue;
                if (!clan.Uloga.Equals("Nema"))
                {

                    model.Clanovi[clan.Uloga].Add(najava);
                }
                else
                    model.Nerazvrstani.Add(najava);

            }
            foreach (var clan in _ctx.ClanNaProjektu.Where(p => p.IdProjekt == dog.IdProjekt).Include(c => c.IdKorisnikNavigation).AsEnumerable())
            {
                if (!clan.Uloga.Equals("Nema"))
                {

                    model.ClanoviProjekta[clan.Uloga].Add(clan);
                }
                else
                    model.NerazvrstaniClanovi.Add(clan);
            }
            model.Evidencija = _ctx.EvidencijaDolaska.Where(e => e.IdDogadjaj == id).Select(e => e.IdKorisnik).ToList();
            var zbor = _ctx.Zbor.Find(dog.IdProjektNavigation.IdZbor);
            ViewData["zborId"] = dog.IdProjektNavigation.IdZbor;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Evidentiraj(DogadjajViewModel model)
        {
            var dog = _ctx.Dogadjaj.Where(d => d.Id == model.IdDogadjaj).Include(d => d.IdProjektNavigation).SingleOrDefault();
            if (dog == null)
                return RedirectToAction("Nema", "Greska");
            if(dog.IdProjektNavigation.Zavrsen)
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!IsAdmin(dog.IdProjektNavigation.IdZbor, user.Id))
                return RedirectToAction("Prava");
            var stare = _ctx.EvidencijaDolaska.Where(d => d.IdDogadjaj == model.IdDogadjaj).ToList();
            _ctx.EvidencijaDolaska.RemoveRange(stare);
            var nove = model.Evidencija.Select(e => new EvidencijaDolaska { Id = Guid.NewGuid(), IdDogadjaj = model.IdDogadjaj, IdKorisnik = e }).ToList();
            _ctx.EvidencijaDolaska.AddRange(nove);
            _ctx.SaveChanges();
            return RedirectToAction("Dogadjaj", new { id = model.IdDogadjaj });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NapustiProjekt(Guid id, ProjektViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var projekt = _ctx.Projekt.Find(id);
            if (projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (projekt.Zavrsen)
                return RedirectToAction("Nema", "Greska");
            var clan = _ctx.ClanNaProjektu.Where(c => c.IdProjekt == id && c.IdKorisnik == user.Id).SingleOrDefault();
            if(clan != null)
            {
                _ctx.ClanNaProjektu.Remove(clan);
                _ctx.SaveChanges();
            }
            var pretplata = _ctx.PretplataNaProjekt.Where(p => p.IdKorisnik == clan.IdKorisnik && p.IdProjekt == clan.IdProjekt).ToList();
            _ctx.RemoveRange(pretplata);
            var pretplatnici = _ctx.ModeratorZbora.Where(p => p.IdZbor == projekt.IdZbor).Select(p => p.IdKorisnik).ToHashSet();
            pretplatnici.Add(_ctx.Voditelj.Where(v => v.IdZbor == projekt.IdZbor).Select(p => p.IdKorisnik).FirstOrDefault());
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret,
                    Tekst = String.Format("<b>{0}</b> napušta projekt <b>{1}</b>.", _ctx.Korisnik.Find(user.Id).ImeIPrezime(), projekt.Naziv),
                    Procitano = false,
                    Poveznica = "/Zbor/AdministracijaProjekta/" + projekt.Id
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }
            _ctx.SaveChanges();
            return RedirectToAction("Projekti", new { id = projekt.IdZbor });
        }
        [HttpPost]
        public async Task<IActionResult> PrihvatiPrijavuProjekt([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            Guid idPrijava;
            var flag = Guid.TryParse(model.Value, out idPrijava);
            if (flag == false)
                return BadRequest();
            PrijavaZaProjekt prijava = _ctx.PrijavaZaProjekt.Where(p => p.Id == idPrijava).Include(p => p.IdKorisnikNavigation).Include(p => p.IdProjektNavigation).SingleOrDefault();
            if (prijava == null)
                return NotFound();
            if (!IsAdmin(prijava.IdProjektNavigation.IdZbor, user.Id))
                return Forbid();
            if (prijava.IdProjektNavigation.Zavrsen)
                return Conflict();
            var cl = _ctx.ClanNaProjektu.Where(c => c.IdProjekt == prijava.IdProjekt && c.IdKorisnik == prijava.IdKorisnik).SingleOrDefault();
            if (cl != null)
                return Ok();
            ClanNaProjektu clan = new ClanNaProjektu
            {
                Id = Guid.NewGuid(),
                Uloga = "Nema",
                IdProjekt = prijava.IdProjekt,
                IdKorisnik = prijava.IdKorisnik
            };
            var pretplata = new PretplataNaProjekt
            {
                Id = Guid.NewGuid(),
                IdProjekt = prijava.IdProjekt,
                IdKorisnik = prijava.IdKorisnik,
                Obavijesti = true,
                Dogadjaji = true
            };
            _ctx.Add(pretplata);
            _ctx.ClanNaProjektu.Add(clan);
            _ctx.PrijavaZaProjekt.Remove(prijava);
            PretplataNaProjekt pret = new PretplataNaProjekt
            {
                Id = Guid.NewGuid(),
                IdKorisnik = prijava.IdKorisnik,
                IdProjekt = prijava.IdProjekt,
                Obavijesti = true,
                Dogadjaji = true
            };
            _ctx.Add(pret);
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = prijava.IdKorisnik,
                Tekst = String.Format("Prihvaćena je prijava za projekt <b>{0}</b>.", _ctx.Projekt.Find(prijava.IdProjekt).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/Projekt/" + prijava.IdProjekt
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(prijava.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            return Ok(new {id=clan.Id, imeIPrezime = _ctx.Korisnik.Find(clan.IdKorisnik).ImeIPrezime(), idkorisnik = clan.IdKorisnik });
        }

        [HttpPost]
        public IActionResult DohvatiStatistiku([FromBody] LajkModel model)
        {
            Guid idKorisnik, idProjekt;
            var flagK = Guid.TryParse(model.IdKorisnik, out idKorisnik);
            var flagP = Guid.TryParse(model.IdCilj, out idProjekt);
            if (flagK == false || flagP == false)
                return BadRequest();
            var dogadaji = _ctx.Dogadjaj.Where(d => d.IdProjekt == idProjekt)
                .Include(d => d.EvidencijaDolaska).OrderByDescending(d => d.DatumIvrijeme);
            var ev = dogadaji.Where(d => d.EvidencijaDolaska.Select(e => e.IdKorisnik).Contains(idKorisnik)).Select(d => new { Id = d.Id, Datum = d.DatumIvrijeme.ToString("dd.MM.yyyy. hh:mm"), Naziv = d.Naziv }).ToList();
            double postotak = 0.0;
            if(dogadaji.Count() > 0)
                postotak = 1.0* ev.Count / (dogadaji.Count()) * 100;
            var response = new
            {
                Evidentirani = ev,
                Neevidentirani = dogadaji.Where(d => !d.EvidencijaDolaska.Select(e => e.IdKorisnik).Contains(idKorisnik)).Select(d => new { Id = d.Id, Datum = d.DatumIvrijeme.ToString("dd.MM.yyyy. hh:mm"), Naziv = d.Naziv }).ToList(),
                Postotak = postotak
            };
            return Ok(response);
        }
        [HttpPost]
        public IActionResult LajkoviObavijest([FromBody] StringModel model)
        {
            Guid idObavijest;
            var flagK = Guid.TryParse(model.Value, out idObavijest);
            if (flagK == false)
                return BadRequest();
            var lajkovi = _ctx.LajkObavijesti.Where(l => l.IdObavijest == idObavijest)
                .Include(d => d.IdKorisnikNavigation).Select(l => new { id = l.IdKorisnik , imeIPrezime=l.IdKorisnikNavigation.ImeIPrezime()}).ToList();
            var response = new
            {
                Lista = lajkovi,
            };
            return Ok(response);
        }
        [HttpPost]
        public IActionResult LajkoviKomentar([FromBody] StringModel model)
        {
            Guid idKomentar;
            var flagK = Guid.TryParse(model.Value, out idKomentar);
            if (flagK == false)
                return BadRequest();
            var lajkovi = _ctx.LajkKomentara.Where(l => l.IdKomentar == idKomentar)
                .Include(d => d.IdKorisnikNavigation).Select(l => new { id = l.IdKorisnik, imeIPrezime = l.IdKorisnikNavigation.ImeIPrezime() });
            var response = new
            {
                Lista = lajkovi,
            };
            return Ok(response);
        }


        [HttpPost]
        public async Task<IActionResult> OdbijPrijavuProjekt([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idPrijava;
            var flag = Guid.TryParse(model.Value, out idPrijava);
            if (flag == false)
                return BadRequest();
            PrijavaZaProjekt prijava = _ctx.PrijavaZaProjekt.Where(p => p.Id == idPrijava).Include(p => p.IdProjektNavigation).SingleOrDefault();
            if (prijava == null)
                return NotFound();
            if (!IsAdmin(prijava.IdProjektNavigation.IdZbor, user.Id))
                return Forbid();
            if (prijava.IdProjektNavigation.Zavrsen)
                return Conflict();
            _ctx.PrijavaZaProjekt.Remove(prijava);
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = prijava.IdKorisnik,
                Tekst = String.Format("Odbijena je prijava za projekt <b>{0}</b>.", _ctx.Projekt.Find(prijava.IdProjekt).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/Projekt/" + prijava.IdProjekt
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(prijava.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            var m = new StringModel { Value = "OKje" };
            return Ok(m);
        }

        [HttpPost]
        public async Task<IActionResult> ObrisiPozivProjekt([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idPoziv;
            var flag = Guid.TryParse(model.Value, out idPoziv);
            if (flag == false)
                return BadRequest();
            PozivZaProjekt poziv = _ctx.PozivZaProjekt.Where(p => p.Id == idPoziv).Include(p => p.IdProjektNavigation).SingleOrDefault();
            if (poziv == null)
                return NotFound();
            if (!IsAdmin(poziv.IdProjektNavigation.IdZbor, user.Id))
                return Forbid();
            if (poziv.IdProjektNavigation.Zavrsen)
                return Conflict();
            _ctx.PozivZaProjekt.Remove(poziv);
            _ctx.SaveChanges();
            var m = new StringModel { Value = "OKje" };

            return Ok(m);
        }
        /*
         * 
         * PAZI OVO JAKO DOBROOO!!
         * */
        public IActionResult OdbijPoziv(Guid id)
        {
            PozivZaZbor poziv = _ctx.PozivZaZbor.Where(p => p.Id == id).SingleOrDefault();
            _ctx.PozivZaZbor.Remove(poziv);
            _ctx.SaveChanges();
            return RedirectToAction("Index");
        }
        public async Task< IActionResult > PrihvatiPoziv(Guid id)
        {
            PozivZaZbor poziv = _ctx.PozivZaZbor.Where(p => p.Id == id).SingleOrDefault();
            _ctx.PozivZaZbor.Remove(poziv);
            ClanZbora clan = new ClanZbora
            {
                Id = Guid.NewGuid(),
                DatumPridruzivanja = DateTime.Now,
                Glas = "sopran",
                IdKorisnik = poziv.IdKorisnik,
                IdZbor = poziv.IdZbor
            };
            PretplataNaZbor pr = new PretplataNaZbor
            {
                Id = Guid.NewGuid(),
                IdKorisnik = poziv.IdKorisnik,
                IdZbor = poziv.IdZbor,
                Obavijesti = true,
                Pitanja = true
            };
            _ctx.ClanZbora.Add(clan);
            _ctx.PretplataNaZbor.Add(pr);
            var pretplatnici = _ctx.ModeratorZbora.Where(p => p.IdZbor == clan.IdZbor).Select(p => p.IdKorisnik).ToHashSet();
            pretplatnici.Add(_ctx.Voditelj.Where(v => v.IdZbor == clan.IdZbor).Select(p => p.IdKorisnik).FirstOrDefault());
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret,
                    Tekst = String.Format("<b>{0}</b> se prihvaća poziv za zbor <b>{1}</b>.", _ctx.Korisnik.Find(poziv.IdKorisnik).ImeIPrezime(), _ctx.Zbor.Find(poziv.IdZbor).Naziv),
                    Procitano = false,
                    Poveznica = "/Korisnik/Index" + poziv.IdKorisnik
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }
            _ctx.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PozivZaZbor([FromBody] PrijavaModel prijava)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid id, idZbor;
            var flagI = Guid.TryParse(prijava.Id, out id);
            var flagZ = Guid.TryParse(prijava.Naziv, out idZbor);
            if (flagI == false || flagZ == false)
                return BadRequest();
            if (!IsAdmin(idZbor, user.Id))
                return Forbid();
            var korisnik = _ctx.Korisnik.Find(id);

            if (!Exists(idZbor) || korisnik == null)
                return NotFound();
            var pozivZaZbor = _ctx.PozivZaZbor.Where(p => p.IdKorisnik == id && p.IdZbor == idZbor).SingleOrDefault();
            if (pozivZaZbor != null)
                return Ok();
            
            var pr = new PozivZaZbor
            {
                Id = Guid.NewGuid(),
                IdKorisnik = id,
                IdZbor = idZbor,
                Poruka = prijava.Poruka,
                DatumPoziva = DateTime.Now
            };
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = id,
                Tekst = String.Format("Pozvani ste u zbor <b>{0}</b>.", _ctx.Zbor.Find(idZbor).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/JavniProfil/" + idZbor
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(id.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.PozivZaZbor.Add(pr);
            _ctx.SaveChanges();
            return Ok(new {ImeIPrezime = korisnik.ImeIPrezime(), Datum=pr.DatumPoziva.ToString("dd.MM.yyyy."), Id = pr.Id.ToString() });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PovuciPrijavu(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var projekt = _ctx.Projekt.Find(id);
            if (projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (!CheckRights(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            if (projekt.Zavrsen)
                return RedirectToAction("Nema", "Greska");
            var prijava = _ctx.PrijavaZaProjekt.Where(p => p.IdProjekt == id && p.IdKorisnik == user.Id).SingleOrDefault();
            if (projekt == null)
                return RedirectToAction("Projekti", new { id = projekt.IdZbor });
            
            _ctx.PrijavaZaProjekt.Remove(prijava);
            _ctx.SaveChanges();

            return RedirectToAction("Projekti", new { id = projekt.IdZbor });
        }
        [HttpPost]
        public async Task<IActionResult> DodajClanProjekt([FromBody] LajkModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid id, idProjekt;
            var flagI = Guid.TryParse(model.IdKorisnik, out id);
            var flagP = Guid.TryParse(model.IdCilj, out idProjekt);
            if (!IsAdmin(_ctx.Projekt.Find(idProjekt).IdZbor, user.Id))
                return Forbid();
            var projekt = _ctx.Projekt.Find(idProjekt);
            if (projekt == null || _ctx.Korisnik.Find(id) == null)
                return NotFound();
            if (projekt.Zavrsen)
                return Conflict();
            var clan = _ctx.ClanNaProjektu.Where(p => p.IdKorisnik == id && p.IdProjekt == idProjekt).Include(p =>p.IdProjektNavigation).SingleOrDefault();
           
            if (clan != null)
                return Ok();
            var pretplata = new PretplataNaProjekt
            {
                Id = Guid.NewGuid(),
                IdProjekt = idProjekt,
                IdKorisnik = id,
                Obavijesti = true,
                Dogadjaji = true
            };
            _ctx.Add(pretplata);
            var c = new ClanNaProjektu
            {
                Id = Guid.NewGuid(),
                IdKorisnik = id,
                IdProjekt = idProjekt,
                Uloga = "Nema"
            };
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = id,
                Tekst = String.Format("Dodani ste na projekt <b>{0}</b>.", _ctx.Projekt.Find(idProjekt).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/Projekt/" + idProjekt
            };
            _ctx.Add(ob);
            await _hubContext.Clients.User(id.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.ClanNaProjektu.Add(c);
            _ctx.SaveChanges();
            return Ok(new { id = c.Id, ImeIPrezime= _ctx.Korisnik.Find(id).ImeIPrezime(), idkorisnik=id});
        }

        [HttpPost]
        public async Task<IActionResult> PretragaKorisnikaProjekt([FromBody] PretragaModel model)
        {
            Guid idProjekt;
            var flag = Guid.TryParse(model.Id, out idProjekt);
            if (!flag)
                return BadRequest();
            var projekt = _ctx.Projekt.Find(idProjekt);
            if (projekt == null)
                return NotFound();
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(projekt.IdZbor, user.Id))
                return Forbid();
            var korisnici = _ctx.Korisnik.Where(k => (k.Ime.Trim().ToLower() + ' ' + k.Prezime.Trim().ToLower()).Contains(model.Tekst))
                .Where(k => k.ClanZbora.Where(c => c.IdZbor == projekt.IdZbor && c.IdKorisnik == k.Id).SingleOrDefault() != null)
                .Where(k => k.ClanNaProjektu.Where(c => c.IdProjekt == projekt.Id && c.IdKorisnik == k.Id).SingleOrDefault() == null && k.PozivZaProjekt.Where(c => c.IdProjekt == projekt.Id && c.IdKorisnik == k.Id).SingleOrDefault() == null && k.PrijavaZaProjekt.Where(c => c.IdProjekt == projekt.Id && c.IdKorisnik == k.Id).SingleOrDefault() == null)
                .Select(k => new { Id = k.Id, ImeIPrezime = k.Ime.Trim() + ' ' + k.Prezime.Trim() })
                .ToList();

            return Ok(korisnici);
        }

        [HttpGet]
        public async Task<IActionResult> NoviDogadjaj(Guid id)
        {
            var projekt = _ctx.Projekt.Find(id);
            if(projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (projekt.Zavrsen)
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            NoviDogadjajViewModel model = new NoviDogadjajViewModel
            {
                IdProjekt = id,
                VrsteDogadjaja = _ctx.VrstaDogadjaja.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Naziv }).ToList()

            };
            var zbor = _ctx.Zbor.Find(projekt.IdZbor);
            ViewData["zborId"] = zbor.Id;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NoviDogadjaj(Guid id, NoviDogadjajViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var projekt = _ctx.Projekt.Find(model.Novi.IdProjekt);
            if (projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            if (model.Novi.Naziv.Trim().Equals(""))
                ModelState.AddModelError("Naziv", "Naziv je obavezan.");

            if (model.Novi.Lokacija.Trim().Equals(""))
                ModelState.AddModelError("Lokacija", "Lokacija je obavezna.");

            if (model.Novi.DodatanOpis.Trim().Equals(""))
                ModelState.AddModelError("DodatanOpis", "Opis je obavezan.");
            if (model.Novi.DatumIvrijeme > model.Novi.DatumIvrijemeKraja)
                ModelState.AddModelError("DatumIvrijeme", "Ispravno upišite datume.");
            if (ModelState.IsValid)
            {
                model.Novi.Id = Guid.NewGuid();
                _ctx.Dogadjaj.Add(model.Novi);
                var pretplatnici = _ctx.PretplataNaProjekt.Where(c => c.IdProjekt == model.Novi.IdProjekt && c.Dogadjaji).ToHashSet();
                foreach (var pret in pretplatnici)
                {
                    OsobneObavijesti ob = new OsobneObavijesti
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = pret.IdKorisnik,
                        Tekst = String.Format("Novi događaj u projektu <b>{0}</b>.",  projekt.Naziv),
                        Procitano = false,
                        Poveznica = "/Zbor/Projekt/" + model.Novi.IdProjekt
                    };
                    _ctx.Add(ob);
                    await _hubContext.Clients.User(pret.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

                }
                _ctx.SaveChanges();
                return RedirectToAction("NoviDogadjaj", new { id = model.Novi.IdProjekt });
            }
            model.IdProjekt = model.Novi.IdProjekt;
            model.VrsteDogadjaja = _ctx.VrstaDogadjaja.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Naziv }).ToList();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> NajavaDolaska([FromBody] LajkModel lajk)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idDogadjaj;
            var flag = Guid.TryParse(lajk.IdCilj, out idDogadjaj);
            if (!flag) return BadRequest();
            var dog = _ctx.Dogadjaj.Where(d => d.Id == idDogadjaj && !d.IdProjektNavigation.Zavrsen).SingleOrDefault();
            if (dog == null)
                return NotFound();
            var l = new NajavaDolaska
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdDogadjaj = idDogadjaj

            };
            _ctx.NajavaDolaska.Add(l);
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiNajavuDolaska([FromBody] LajkModel lajk)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idDogadjaj;
            var flag = Guid.TryParse(lajk.IdCilj, out idDogadjaj);
            if (!flag) return BadRequest();
            var dog = _ctx.Dogadjaj.Where(d => d.Id == idDogadjaj && !d.IdProjektNavigation.Zavrsen).SingleOrDefault();
            if (dog == null)
                return NotFound();
            var l = _ctx.NajavaDolaska.Where(l => l.IdKorisnik == user.Id && l.IdDogadjaj == idDogadjaj).SingleOrDefault();
            if (l == null)
                return NotFound();
            _ctx.NajavaDolaska.Remove(l);
            _ctx.SaveChanges();
            return Ok();

        }

        [HttpGet]
        public async Task<IActionResult> Kalendar(Guid id)
        {
            if(!Exists(id))
                return RedirectToAction("Nema", "Greska");
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
            var dogadjaji = _ctx.Dogadjaj.Where(d => d.IdProjektNavigation.IdZbor == id)
                .Select(d => new DogadjajModel
                {
                    Id = d.Id.ToString(),
                    Title = "[" + d.IdProjektNavigation.Naziv + "]\n" + d.Naziv.ToString(),
                    Start = d.DatumIvrijeme.ToString("yyyy-MM-dd HH:mm:ss"),
                    End = d.DatumIvrijemeKraja.ToString("yyyy-MM-dd HH:mm:ss"),
                    BackgroundColor = "rgb(" + ((d.IdVrsteDogadjaja.GetHashCode()>>16)&0xFF )+ "," + ((d.IdVrsteDogadjaja.GetHashCode() >> 8) & 0xFF) + "," + (d.IdVrsteDogadjaja.GetHashCode() & 0xFF) + ")"
                }).ToList() ;
            //KalendarViewModel model = new KalendarViewModel();
            string jsonString = JsonConvert.SerializeObject(dogadjaji);
            var zbor = _ctx.Zbor.Find(id);
            ViewData["zborId"] = id;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            return View(new KalendarViewModel {Dogadjaji = dogadjaji, IdZbor = id });
        }
        [HttpPost]
        public async Task<IActionResult> UcitajClanove(Guid id, IFormFile file)
        {
            if(!Exists(id))
                return RedirectToAction("Nema", "Greska");
            ApplicationUser u = await _userManager.GetUserAsync(HttpContext.User);
            if (!IsAdmin(id, u.Id))
                return RedirectToAction("Prava");
            if (!file.FileName.EndsWith("xlsx"))
            {
                TempData["mess"] = "Uploadajte u xlsx formatu.";
            }
            else if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        // Choose one of either 1 or 2:

                        // 1. Use the reader methods
                        do
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    string glas = reader.GetString(4);

                                    if (glas == null) glas = "ne";
                                    else glas.Trim().ToLower();
#pragma warning disable CS0642 // Possible mistaken empty statement
                                    if (glas.Equals("sopran") || glas.Equals("alt") || glas.Equals("tenor") || glas.Equals("bas")) ;
#pragma warning restore CS0642 // Possible mistaken empty statement
                                    else glas = "ne";
                                    string email = reader.GetString(2);
                                    var user = await _userManager.FindByEmailAsync(email);
                                    if (user == null)
                                    {
                                        user = new ApplicationUser { UserName = email, Email = email };
                                        var password = RandomString();
                                        var korisnik = new Korisnik
                                        {
                                            Id = Guid.NewGuid(),
                                            Ime = reader.GetString(0),
                                            Prezime = reader.GetString(1),
                                            DatumRodjenja = DateTime.Parse(reader.GetString(3)),
                                        };

                                        var result = await _userManager.CreateAsync(user, password);
                                        korisnik.Id = user.Id;
                                        korisnik.ClanZbora.Add(new ClanZbora { DatumPridruzivanja = DateTime.Now, IdKorisnik = user.Id, IdZbor = id, Id = Guid.NewGuid(), Glas = glas });
                                        korisnik.PretplataNaZbor.Add(new PretplataNaZbor { Id = Guid.NewGuid(), IdKorisnik = user.Id, IdZbor = id, Obavijesti = true, Pitanja = true, Repozitorij = true });
                                        _ctx.Add(korisnik);
                                        OsobneObavijesti ob = new OsobneObavijesti
                                        {
                                            Id = Guid.NewGuid(),
                                            IdKorisnik = user.Id,
                                            Tekst = String.Format("Dodani ste u zbor <b>{0}</b>.", _ctx.Zbor.Find(id).Naziv),
                                            Procitano = false,
                                            Poveznica = "/Zbor/Profil/" + id
                                        };
                                        _ctx.Add(ob);
                                        await _hubContext.Clients.User(user.Id.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
                                        await _emailSender.SendEmailAsync(user.Email, "", "Vaša lozinka je: " + password);
                                    }
                                
                                
                                    else
                                    {
                                        var clan = _ctx.ClanZbora.Where(c => c.IdKorisnik == user.Id && c.IdZbor == id).FirstOrDefault();

                                        if (clan == null)
                                        {
                                            _ctx.Add(new ClanZbora { DatumPridruzivanja = DateTime.Now, IdKorisnik = user.Id, IdZbor = id, Id = Guid.NewGuid(), Glas = glas });
                                            _ctx.Add(new PretplataNaZbor { Id = Guid.NewGuid(), IdKorisnik = user.Id, IdZbor = id, Obavijesti = true, Pitanja = true, Repozitorij = true });
                                            OsobneObavijesti ob = new OsobneObavijesti
                                            {
                                                Id = Guid.NewGuid(),
                                                IdKorisnik = user.Id,
                                                Tekst = String.Format("Dodani ste u zbor <b>{0}</b>.", _ctx.Zbor.Find(id).Naziv),
                                                Procitano = false,
                                                Poveznica = "/Zbor/Profil/" + id
                                            };
                                            _ctx.Add(ob);
                                            await _hubContext.Clients.User(user.Id.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    TempData["mess"]="Došlo je do greške";
                                }

                            }
                        } while (reader.NextResult());
                    }
                    _ctx.SaveChanges();

                }

            }
            else
            {
                TempData["mess"] = "File je prazan";
            }
            return RedirectToAction("Administracija", new { id = id});
        }
        private string RandomString(int size = 6, bool lowerCase = true)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Prava()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Projekt(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var kor = _ctx.Korisnik.Find(user.Id);
            var projekt = _ctx.Projekt.Where(p => p.Id == id).Include(p => p.IdVrstePodjeleNavigation).Include(p => p.Dogadjaj).ThenInclude(d => d.NajavaDolaska).SingleOrDefault();
            if (projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (!CheckRights(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");

            var model = new ProjektViewModel { Admin = IsAdmin(projekt.IdZbor, user.Id), Projekt = projekt };
            model.AktivniDogadjaji = projekt.Dogadjaj.Where(d => d.DatumIvrijeme > DateTime.Now).OrderBy(d => d.DatumIvrijeme).AsEnumerable();
            model.ProsliDogadjaji =     projekt.Dogadjaj.Where(d => d.DatumIvrijeme <= DateTime.Now).OrderByDescending(d => d.DatumIvrijeme).AsEnumerable();
            model.IdKorisnik = user.Id;
            var obavijesti = _ctx.Obavijest.Where(o => o.ObavijestVezanaUzProjekt.Select(op => op.IdProjekt).Contains(id))
                .Include(o => o.IdKorisnikNavigation)
                .Include(o => o.LajkObavijesti)
                .Include(o => o.KomentarObavijesti).ThenInclude(k => k.LajkKomentara)
                .Include(o => o.KomentarObavijesti).ThenInclude(k => k.IdKorisnikNavigation)
                 .Include(o => o.KomentarObavijesti).OrderBy(d => d.DatumObjave)
                .OrderByDescending(O => O.DatumObjave);
            model.Obavijesti = obavijesti;
            model.Slika = kor.IdSlika;
            model.ImeIPrezime = kor.ImeIPrezime();
            var clan = _ctx.ClanNaProjektu.Where(c => c.IdProjekt == id && c.IdKorisnik == user.Id).SingleOrDefault();
            model.Clan = clan != null ? true : false;

            var zbor = _ctx.Zbor.Find(projekt.IdZbor);
            ViewData["zborId"] = projekt.IdZbor;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;

            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> NapustiZbor([FromBody] StringModel model) 
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idZbor;
            bool flag = Guid.TryParse(model.Value, out idZbor);
            if (flag == false)
                return BadRequest();
            if (!Exists(idZbor))
                return NotFound();
            var voditelj = _ctx.Voditelj.Where(v => v.IdZbor == idZbor && v.IdKorisnik == user.Id).SingleOrDefault();
            if (voditelj != null)
                return Conflict();
            var clan = _ctx.ClanZbora.Where(c => c.IdKorisnik == user.Id && c.IdZbor == idZbor).SingleOrDefault();
            if(clan != null)
                _ctx.Remove(clan);
            var pretplata = _ctx.PretplataNaZbor.Where(p => p.IdKorisnik == clan.IdKorisnik && p.IdZbor == clan.IdZbor).ToList();
            _ctx.RemoveRange(pretplata);
            var mod = _ctx.ModeratorZbora.Where(c => c.IdKorisnik == user.Id && c.IdZbor == idZbor).SingleOrDefault();
            if(mod!=null)
                _ctx.Remove(mod);
            var pretplatnici = _ctx.ModeratorZbora.Where(p => p.IdZbor == idZbor).Select(p => p.IdKorisnik).ToHashSet();
            pretplatnici.Add(_ctx.Voditelj.Where(v => v.IdZbor == idZbor).Select(p => p.IdKorisnik).FirstOrDefault());
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret,
                    Tekst = String.Format("<b>{0}</b> napušta zbor <b>{1}</b>.", _ctx.Korisnik.Find(user.Id).ImeIPrezime(), _ctx.Zbor.Find(idZbor).Naziv),
                    Procitano = false,
                    Poveznica = "/Zbor/Administracija/" + idZbor
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }
            _ctx.SaveChanges();
            return Ok(new { ok = 42 });
        }
        [HttpGet]
        public async Task<IActionResult> ObrisiDogadjaj(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dog = _ctx.Dogadjaj.Where(c => c.Id == id).SingleOrDefault();
            _ctx.Remove(dog);
            var pretplatnici = _ctx.PretplataNaProjekt.Where(p => p.IdProjekt == dog.IdProjekt && p.Dogadjaji).ToHashSet();
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret.IdKorisnik,
                    Tekst = String.Format("Događaj <b>{0}</b> je obrisan.", dog.Naziv),
                    Procitano = false,
                    Poveznica = "/Zbor/Projekt/" + dog.IdProjekt
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }
            _ctx.SaveChanges();
            return RedirectToAction("Projekt" ,new { id = dog.IdProjekt });
        }
        [HttpGet]
        public async Task<IActionResult> UrediDogadjaj(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dog = _ctx.Dogadjaj.Where(c => c.Id == id).Include(p => p.IdProjektNavigation).SingleOrDefault();
            if (dog == null)
                return RedirectToAction("Nema", "Greska");
            if (!CheckRights(dog.IdProjektNavigation.IdZbor, user.Id))
                return RedirectToAction("Prava");
            var model = new NoviDogadjajViewModel
            {
                IdProjekt = dog.IdProjekt,
                Novi = dog,
                VrsteDogadjaja = _ctx.VrstaDogadjaja.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Naziv }).ToList()

            };
            var pretplatnici = _ctx.PretplataNaProjekt.Where(p => p.IdProjekt == dog.IdProjekt && p.Dogadjaji).ToList();
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret.IdKorisnik,
                    Tekst = String.Format("Događaj <b>{0}</b> je uređen.", dog.Naziv),
                    Procitano = false,
                    Poveznica = "/Zbor/Dogadjaj/" + dog.Id
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }
            var zbor = _ctx.Zbor.Find(dog.IdProjektNavigation.IdZbor);
            ViewData["zborId"] = zbor.Id;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            return View(model);
        }
        
        [HttpGet("download")]
        public async Task<IActionResult> SkiniKalendar(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
            var dogadjaji = _ctx.Dogadjaj.Where(d => d.IdProjektNavigation.IdZbor == id).Include(p => p.IdProjektNavigation).ToList();

            var calendar = new Calendar();
            foreach (var dog in dogadjaji)
            {
                var e = new CalendarEvent
                {
                    Start = new CalDateTime(dog.DatumIvrijeme),
                    End = new CalDateTime(dog.DatumIvrijemeKraja),
                    Uid = dog.Id+"Zboris",
                    Location = dog.Lokacija,
                    Name = dog.Naziv + "["+dog.IdProjektNavigation.Naziv+"]"
                };
                calendar.Events.Add(e);

            }
            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(calendar);
            byte[] bytes = Encoding.UTF8.GetBytes(serializedCalendar);

            return File(bytes, "text/calendar", "kalendar.ical");
        }
        [HttpGet]
        public async Task<IActionResult> JavniProfil(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            var zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.ProfilZbor)
                .Include(z => z.Voditelj).ThenInclude(v => v.IdKorisnikNavigation).SingleOrDefault();
            
            JavniProfilViewModel model = new JavniProfilViewModel
            {
                Zbor = zbor,
                Mod = IsAdmin(id, user.Id)
            };
            if(CheckRights(id, user.Id))
            {
                ViewData["zborId"] = id;
                ViewData["zborIme"] = zbor.Naziv;
                ViewData["idSlika"] = zbor.IdSlika;
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Urediozboru([FromBody] PretragaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idZbor;
            bool flag = Guid.TryParse(model.Id, out idZbor);
            if (flag == false)
                return BadRequest();
            if (!Exists(idZbor))
                return NotFound();
            if (!IsAdmin(idZbor, user.Id))
                return Forbid();
            _ctx.ProfilZbor.Find(idZbor).OZboru = model.Tekst;
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Urediovoditeljima([FromBody] PretragaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idZbor;
            bool flag = Guid.TryParse(model.Id, out idZbor);
            if (flag == false)
                return BadRequest();
            if (!Exists(idZbor))
                return NotFound();
            if (!IsAdmin(idZbor, user.Id))
                return Forbid();
            _ctx.ProfilZbor.Find(idZbor).OVoditeljima = model.Tekst;
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Uredirepertoar([FromBody] PretragaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idZbor;
            bool flag = Guid.TryParse(model.Id, out idZbor);
            if (flag == false)
                return BadRequest();
            if (!Exists(idZbor))
                return NotFound();
            if (!IsAdmin(idZbor, user.Id))
                return Forbid();
            _ctx.ProfilZbor.Find(idZbor).Repertoar = model.Tekst;
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> Uredireprezentacija([FromBody] PretragaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idZbor;
            bool flag = Guid.TryParse(model.Id, out idZbor);
            if (flag == false)
                return BadRequest();
            if (!Exists(idZbor))
                return NotFound();
            if (!IsAdmin(idZbor, user.Id))
                return Forbid();
            _ctx.ProfilZbor.Find(idZbor).Reprezentacija = model.Tekst;
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> Pretplate(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
            var zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.PretplataNaZbor).Include(z => z.Projekt).ThenInclude(p => p.PretplataNaProjekt).SingleOrDefault();
            
            PretplateViewModel model = new PretplateViewModel
            {
                Zbor = zbor,
                Projekti = zbor.Projekt.ToList(),
                PretplataZbor = zbor.PretplataNaZbor.Where(p => p.IdKorisnik == user.Id).SingleOrDefault(),
                PretplataProjekt = zbor.Projekt.Select(p => p.PretplataNaProjekt.Where(p => p.IdKorisnik == user.Id).FirstOrDefault()).ToList()
            };
            ViewData["zborId"] = id;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Pretplate(Guid id, PretplateViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
            var pretplataZbor = _ctx.PretplataNaZbor.Where(z => z.IdZbor == id && z.IdKorisnik == user.Id).SingleOrDefault();
             if(pretplataZbor == null)
            {
                pretplataZbor = new PretplataNaZbor
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = user.Id,
                    IdZbor = id,
                    Obavijesti = model.PretplataZbor.Obavijesti,
                    Repozitorij = model.PretplataZbor.Repozitorij,
                    Pitanja = model.PretplataZbor.Pitanja
                };
                _ctx.PretplataNaZbor.Add(pretplataZbor);
            }
            else
            {
                pretplataZbor.Obavijesti = model.PretplataZbor.Obavijesti;
                pretplataZbor.Pitanja = model.PretplataZbor.Pitanja;
                pretplataZbor.Repozitorij = model.PretplataZbor.Repozitorij;

            }
            foreach(var pretplata in model.PretplataProjekt)
            {
                var pretplataProjekt = _ctx.PretplataNaProjekt.Where(z => z.IdProjekt == pretplata.Id && z.IdKorisnik == user.Id).SingleOrDefault();
                if (pretplataProjekt == null)
                {
                    pretplataProjekt = new PretplataNaProjekt
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = user.Id,
                        IdProjekt = pretplata.IdProjekt,
                        Obavijesti =pretplata.Obavijesti,
                        Dogadjaji =pretplata.Dogadjaji
                    };
                    _ctx.PretplataNaProjekt.Add(pretplataProjekt);
                }
                else
                {
                    pretplataProjekt.Obavijesti = pretplata.Obavijesti;
                    pretplataProjekt.Dogadjaji = pretplata.Dogadjaji;

                }
                _ctx.SaveChanges();
            }


            return RedirectToAction("Pretplate", new { id = id });
        }
        public async Task<IActionResult> Galerija(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            var slike = _ctx.RepozitorijZbor.Where(z => z.IdZbor == id).ToList().Where(s => s.JeSlika()).ToList();
            var clan = CheckRights(id, user.Id);
            if(clan == false)
            {
                slike = slike.Where(s => s.Privatno == false).ToList();
            }
            GalerijaViewModel model = new GalerijaViewModel
            {
                Slike = slike,
                Clan = clan,
                IdZbor = id
            };
            if(clan)
            {
                var zbor = _ctx.Zbor.Find(id);
                ViewData["zborId"] = id;
                ViewData["zborIme"] = zbor.Naziv;
                ViewData["idSlika"] = zbor.IdSlika;
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiZbor(AdministracijaViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var zbor = _ctx.Zbor.Where(z => z.Id == model.IdBrisanje).Include(z => z.RepozitorijZbor).Include(z => z.Projekt).Include(z => z.Voditelj).SingleOrDefault();
            if (zbor == null)
                return RedirectToAction("Nema", "Greska");
            if (zbor.Voditelj.Where(v => v.IdKorisnik == user.Id && v.IdZbor == model.IdBrisanje).SingleOrDefault() == null)
                return RedirectToAction("Prava");
            foreach (var p in zbor.Projekt)
                _ctx.Remove(p);
            DeleteAll(model.IdBrisanje);
            var pretplatnici = _ctx.ClanZbora.Where(c => c.IdZbor == zbor.Id).ToList();
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret.IdKorisnik,
                    Tekst = String.Format("Zbor <b>{0}</b> je obrisan.", zbor.Naziv),
                    Procitano = false,
                    Poveznica = "/Home/Index"
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }
            _ctx.Remove(zbor);

            _ctx.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
        private async void DeleteAll(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.RepozitorijZbor).Include(z => z.Voditelj).SingleOrDefault();
            if (zbor == null)
                return;
            if (zbor.Voditelj.Where(v => v.IdKorisnik == user.Id && v.IdZbor == id).SingleOrDefault() == null)
                return;
            foreach (var dat in zbor.RepozitorijZbor)
            {
                try
                {

                    System.IO.File.Delete(LOCATION + "/" + dat.Url);
                }
                catch (Exception)
                {
                    continue;
                };
            }
            System.IO.Directory.Delete(LOCATION + "/Zbor/" + zbor.Id);


            return;
        }
    }
}
