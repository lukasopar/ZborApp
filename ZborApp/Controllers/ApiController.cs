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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ZborDataStandard.ViewModels.JSONModels;
using ZborApp.Services;
using ZborDataStandard.Account;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.KorisnikVIewModels;
using ZborDataStandard.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ZborDataStandard.ViewModels.ZborViewModels;

namespace ZborApp.Controllers
{
    [Authorize(AuthenticationSchemes =
    JwtBearerDefaults.AuthenticationScheme)]
    public class ApiController : Controller
    {
        private readonly AppSettings appData = new AppSettings();

        private readonly ILogger<KorisnikController> _logger;
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private IUserService _userService;
        public ApiController(ILogger<KorisnikController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
            _userService = userService;
            _emailSender = new EmailSender();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Token([FromBody]LoginViewModel model)
        {
            var user = await _userService.Authenticate(model.Email, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }
        [AllowAnonymous]
        public IActionResult Pozdrav()
        {
            return Ok("Pozdrav, vojnici");
        }
        public IActionResult PozdravNope()
        {
            return Ok(User.Identity.Name);
        }
        private bool Exists(Guid idZbor)
        {
            var zbor = _ctx.Zbor.Where(z => z.Id == idZbor).AsNoTracking().SingleOrDefault();
            return zbor == null ? false : true;
        }
        private bool CheckRights(Guid idZbor, Guid idKorisnik)
        {
            var clan = _ctx.ClanZbora.Where(c => c.IdKorisnik == idKorisnik && c.IdZbor == idZbor).AsNoTracking().SingleOrDefault();
            var voditelj = _ctx.Voditelj.Where(v => v.IdZbor == idZbor).OrderByDescending(v => v.DatumPostanka).AsNoTracking().SingleOrDefault();
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

        public IActionResult Zborovi()
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            var korisnik = _ctx.Korisnik.Where(k => k.Id == user.Id).SingleOrDefault();
            //ispravi
            var mojiZborovi = _ctx.Zbor.Where(z => z.Voditelj.Select(v => v.IdKorisnik).Contains(user.Id) || z.ClanZbora.Select(v => v.IdKorisnik).Contains(user.Id)).ToList();
            var prijaveZborovi = _ctx.Zbor.Where(z => z.PrijavaZaZbor.Select(p => p.IdKorisnik).Contains(user.Id)).Include(p => p.PrijavaZaZbor).ToList();
            var ostaliZborovi = _ctx.Zbor.Where(z => !z.Voditelj.Select(v => v.IdKorisnik).Contains(user.Id) && !z.ClanZbora.Select(v => v.IdKorisnik).Contains(user.Id) && !z.PrijavaZaZbor.Select(p => p.IdKorisnik).Contains(user.Id) && !z.PozivZaZbor.Select(p => p.IdKorisnik).Contains(user.Id)).ToList();
            var mojiPozivi = _ctx.PozivZaZbor.Where(p => p.IdKorisnik == korisnik.Id).Include(p => p.IdZborNavigation).OrderByDescending(p => p.DatumPoziva).ToList();

            IndexViewModel model = new IndexViewModel { MojiPozivi = mojiPozivi, MojiZborovi = mojiZborovi, PoslanePrijaveZborovi = prijaveZborovi, OstaliZborovi = ostaliZborovi, KorisnikId = user.Id };
            return Ok(model);
        }
        [HttpGet]
        public IActionResult Obavijesti(Guid id)
        {
            if (!Exists(id))
                return NotFound();
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            if (!CheckRights(id, user.Id))
                return Forbid();
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
            return Ok(model);
        }
        [HttpPost]
        public IActionResult LajkObavijesti([FromBody] LajkModel lajk)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
                Tekst = String.Format("<b>{0}</b> označava tvoju objavu sa sviđa mi se.", _ctx.Korisnik.Find(user.Id).ImeIPrezime()),
                Procitano = false,
                Poveznica = "/Zbor/Profil/" + obavijest.IdZbor
            };

            _ctx.Add(ob);
            /*await _hubContext.Clients.User(obavijest.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == obavijest.IdZbor).AsEnumerable())
            {
                await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkObavijesti", new { id = idObavijest, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk = true, brojLajkova = brojLajkova + 1 }); ;
            }*/
            _ctx.LajkObavijesti.Add(l);
            _ctx.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult UnlajkObavijesti([FromBody] LajkModel lajk)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
                /*foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == obavijest.IdZbor).AsEnumerable())
                {
                    await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkObavijesti", new { id = idObavijest, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk = false, brojLajkova = brojLajkova - 1 }); ;
                }*/
                _ctx.LajkObavijesti.Remove(l);
                _ctx.SaveChanges();
            }
            return Ok();

        }
        [HttpPost]
        public async Task<IActionResult> LajkKomentara([FromBody] LajkModel lajk)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
            //await _hubContext.Clients.User(komentar.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            var brojLajkova = _ctx.LajkKomentara.Where(l => l.IdKomentar == idKomentar).Count();
           /* foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == komentar.IdObavijestNavigation.IdZbor).AsEnumerable())
            {
                await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkKomentara", new { id = idKomentar, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk = true, brojLajkova = brojLajkova + 1 }); ;
            }*/
            _ctx.LajkKomentara.Add(l);
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> UnlajkKomentara([FromBody] LajkModel lajk)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
            if (l != null)
            {
                var brojLajkova = _ctx.LajkKomentara.Where(l => l.IdKomentar == idKomentar).Count();
               /* foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == komentar.IdObavijestNavigation.IdZbor).AsEnumerable())
                {
                    await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkKomentara", new { id = idKomentar, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk = false, brojLajkova = brojLajkova - 1 }); ;
                }*/
                _ctx.LajkKomentara.Remove(l);
                _ctx.SaveChanges();
            }

            return Ok();

        }
        [HttpPost]
        public IActionResult NoviKomentar([FromBody] NoviKomentarModel komentar)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
            /*await _hubContext.Clients.User(obavijest.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == obavijest.IdZbor).AsEnumerable())
            {
                await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("NoviKomentar", kom);
            }*/
            _ctx.KomentarObavijesti.Add(k);
            _ctx.SaveChanges();
            return Ok(k);
        }
        [HttpPost]
        public  IActionResult NovaObavijest(Guid id, [FromBody]ProfilViewModel model)
        {
            if (!Exists(id))
                return NotFound();
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            if (!CheckRights(id, user.Id))
                return Forbid();
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
                    foreach (var projektId in projekti)
                    {
                        var idProj = Guid.Parse(projektId);
                        var pro = _ctx.Projekt.Find(idProj);
                        if (pro == null || pro.IdZbor != id)
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
                foreach (var pret in pretplatnici)
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
                    //await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });


                }
                _ctx.SaveChanges();
                return Ok(model.NovaObavijest);
            }
            TempData["Model"] = "Ispravite greške unutar polja forme.";
            return RedirectToAction("Profil", new { id = id });
        }
        [HttpGet]
        public IActionResult Pitanja(Guid id)
        {
            if (!Exists(id))
                return NotFound();
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            if (!CheckRights(id, user.Id))
                return Forbid();

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
     
            return Ok(model);
        }
        [HttpPost]
        public IActionResult OdgovoriNaPitanje([FromBody] ListaModel model)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
        public IActionResult NovoPitanje(Guid id, [FromBody] Anketa pitanje)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            pitanje.Id = Guid.NewGuid();
            pitanje.IdKorisnik = user.Id;
            pitanje.DatumPostavljanja = DateTime.Now;
            foreach(var odg in pitanje.OdgovorAnkete)
            {
                odg.Id = Guid.NewGuid();
                odg.IdAnketa = pitanje.Id;
            }
          
            _ctx.Anketa.Add(pitanje);
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
                //await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }

            _ctx.SaveChanges();
            return Ok();
        }
        [HttpGet]
        public IActionResult Administracija(Guid id)
        {
            if (!Exists(id))
                return NotFound();
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            if (!IsAdmin(id, user.Id))
                return Forbid();
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
            return Ok(model);
        }
        
        [HttpPost]
        public IActionResult PrihvatiPrijavu([FromBody] StringModel model)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
                Tekst = String.Format("Prihvaćena prijava za zbor <b>{0}</b>.", _ctx.Zbor.Find(prijava.IdZbor).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/Profil/" + prijava.IdZbor
            };
            _ctx.Add(ob);
            //await _hubContext.Clients.User(prijava.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            clan.IdKorisnikNavigation = _ctx.Korisnik.Find(clan.IdKorisnik);
            return Ok(clan);
        }
        [HttpPost]
        public IActionResult PromjenaGlasa([FromBody] PrijavaModel model)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
        public IActionResult OdbijPrijavu([FromBody] StringModel model)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
            //await _hubContext.Clients.User(prijava.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }
        [HttpPost]
        public IActionResult NoviModerator([FromBody] LajkModel model)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
            //await _hubContext.Clients.User(idMod.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            return Ok(mod);
        }
        [HttpPost]
        public IActionResult ObrisiModeratora([FromBody] StringModel model)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
            //await _hubContext.Clients.User(mod.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
     
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }
        [HttpPost]
        public IActionResult PostaviVoditelja([FromBody]AdministracijaViewModel modell)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            var clan = _ctx.ClanZbora.Find(modell.IdBrisanje);
            if (clan == null)
                return RedirectToAction("Nema", "Greska");
            var vod = _ctx.Voditelj.Where(v => v.IdKorisnik == user.Id && v.IdZbor == clan.IdZbor).SingleOrDefault();
            if (vod == null)
            {
                return Forbid();
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
            foreach (var cl in clanovi)
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
                //await _hubContext.Clients.User(cl.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            }
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public IActionResult ObrisiClana([FromBody]AdministracijaViewModel model)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            var clan = _ctx.ClanZbora.Find(model.IdBrisanje);
            if (clan == null)
                return NotFound();
            if (clan.IdKorisnik == user.Id)
                return Forbid();
            if (!IsAdmin(clan.IdZbor, user.Id))
                return Forbid();
            _ctx.ClanZbora.Remove(clan);
            var pretplata = _ctx.PretplataNaZbor.Where(p => p.IdKorisnik == clan.IdKorisnik && p.IdZbor == clan.IdZbor).ToList();
            _ctx.RemoveRange(pretplata);
            var mod = _ctx.ModeratorZbora.Where(mod => mod.IdKorisnik == clan.IdKorisnik && mod.IdZbor == clan.IdZbor).SingleOrDefault();
            if (mod != null)
                _ctx.ModeratorZbora.Remove(mod);
            OsobneObavijesti ob = new OsobneObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = clan.IdKorisnik,
                Tekst = String.Format("Izbačeni ste iz zbora <b>{0}</b>.", _ctx.Zbor.Find(clan.IdZbor).Naziv),
                Procitano = false,
                Poveznica = "/Zbor/JavniProfil/" + clan.IdZbor
            };
            _ctx.Add(ob);
            //await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            return Ok();
        }



        [HttpPost]
        public IActionResult ObrisiPoziv([FromBody] StringModel model)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
                .ToList();

            return Ok(korisnici);
        }
        [HttpPost]
        public IActionResult PozivZaZbor([FromBody] PrijavaModel prijava)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
            //await _hubContext.Clients.User(id.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.PozivZaZbor.Add(pr);
            _ctx.SaveChanges();
            pr.IdKorisnikNavigation = _ctx.Korisnik.Find(pr.IdKorisnik);
            return Ok(pr);
        }
        [HttpGet]
        public IActionResult Projekti(Guid id)
        {
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");

            var mojiProjekti = _ctx.Projekt.Where(p => id == p.IdZbor && !p.Zavrsen).Where(z => z.ClanNaProjektu.Select(v => v.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var prijaveProjekti = _ctx.Projekt.Where(p => id == p.IdZbor && !p.Zavrsen).Where(z => z.PrijavaZaProjekt.Select(p => p.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var ostaliProjekti = _ctx.Projekt.Where(p => id == p.IdZbor && !p.Zavrsen).Where(z => !z.ClanNaProjektu.Select(v => v.IdKorisnik).Contains(user.Id) && !z.PrijavaZaProjekt.Select(p => p.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var zavrseniProjekti = _ctx.Projekt.Where(p => id == p.IdZbor && p.Zavrsen).ToList();
            ProjektiMobViewModel model = new ProjektiMobViewModel
            {
                IdZbor = id,
                MojiProjekti = mojiProjekti,
                IdKorisnik = user.Id,
                Admin = IsAdmin(id, user.Id),
                OstaliProjekti = ostaliProjekti,
                PrijavaProjekti = prijaveProjekti,
                Projekti = _ctx.Projekt.Where(p => p.IdZbor == id).Include(p => p.ClanNaProjektu).Include(p => p.PrijavaZaProjekt),
                VrstePodjele = _ctx.VrstaPodjele.ToList(),
                ZavrseniProjekti = zavrseniProjekti
            };
            var zbor = _ctx.Zbor.Find(id);
            return Ok(model);
        }


        [HttpPost]
        public IActionResult Projekti([FromBody] Projekt model)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            Projekt Novi = new Projekt
            {
                Id = Guid.NewGuid(),
                DatumPocetka = model.DatumPocetka,
                IdVrstePodjele = model.IdVrstePodjeleNavigation.Id,
                Naziv = model.Naziv,
                Opis = model.Opis,
                IdZbor = model.IdZbor

            };
            if (!Exists(Novi.IdZbor))
                return NotFound();
            if (!CheckRights(Novi.IdZbor, user.Id))
                return Forbid();

            if (model.Naziv.Trim().Equals(""))
                ModelState.AddModelError("Naziv", "Naziv je obavezan");
            if (model.Opis.Trim().Equals(""))
                ModelState.AddModelError("Opis", "Opis je obavezan");

            if (ModelState.IsValid)
            {
                Novi.ClanNaProjektu.Add(new ClanNaProjektu { Id = Guid.NewGuid(), IdKorisnik = user.Id, IdProjekt = Novi.Id, Uloga = "Nema" });
                _ctx.Add(Novi);
                var clanovi = _ctx.ClanZbora.Where(c => c.IdZbor == Novi.IdZbor).ToList();
                foreach (var cl in clanovi)
                {
                    OsobneObavijesti ob = new OsobneObavijesti
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = cl.IdKorisnik,
                        Tekst = String.Format("Dodan je projekt <b>{0}</b> u zbor <b>{1}</b>.", Novi.Naziv, _ctx.Zbor.Find(Novi.IdZbor).Naziv),
                        Procitano = false,
                        Poveznica = "/Zbor/Projekti/" + cl.IdZbor
                    };
                    _ctx.Add(ob);
                    //await _hubContext.Clients.User(cl.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
                }
                _ctx.SaveChanges();
                return Ok(Novi);
            }
            return NoContent();


        }
        [HttpPost]
        public IActionResult PrijavaZaProjekt([FromBody] PrijavaModel prijava)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
                //await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            }
            _ctx.SaveChanges();
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }
        [HttpPost]
        public IActionResult PovuciPrijavu(Guid id)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            var projekt = _ctx.Projekt.Find(id);
            if (projekt == null)
                return NotFound();
            if (!CheckRights(projekt.IdZbor, user.Id))
                return Forbid();
            if (projekt.Zavrsen)
                return NotFound();
            var prijava = _ctx.PrijavaZaProjekt.Where(p => p.IdProjekt == id && p.IdKorisnik == user.Id).SingleOrDefault();
            if (projekt == null)
                return RedirectToAction("Projekti", new { id = projekt.IdZbor });

            _ctx.PrijavaZaProjekt.Remove(prijava);
            _ctx.SaveChanges();

            return Ok();
        }
        [HttpGet]
        public IActionResult Projekt(Guid id)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
            var kor = _ctx.Korisnik.Find(user.Id);
            var projekt = _ctx.Projekt.Where(p => p.Id == id).Include(p => p.IdVrstePodjeleNavigation).Include(p => p.Dogadjaj).ThenInclude(d => d.NajavaDolaska).SingleOrDefault();
            if (projekt == null)
                return NotFound();
            if (!CheckRights(projekt.IdZbor, user.Id))
                return Forbid();

            var model = new ProjektViewModel { Admin = IsAdmin(projekt.IdZbor, user.Id), Projekt = projekt };
            model.AktivniDogadjaji = projekt.Dogadjaj.Where(d => d.DatumIvrijeme > DateTime.Now).OrderBy(d => d.DatumIvrijeme).AsEnumerable();
            model.ProsliDogadjaji = projekt.Dogadjaj.Where(d => d.DatumIvrijeme <= DateTime.Now).OrderByDescending(d => d.DatumIvrijeme).AsEnumerable();
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
            return Ok(model);

        }
        [HttpPost]
        public IActionResult ObrisiDogadjaj(Guid id)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
                //await _hubContext.Clients.User(pret.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> NajavaDolaska([FromBody] LajkModel lajk)
        {
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
            var user = new { Id = Guid.Parse(User.Identity.Name) };
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
    }
}
