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
using JavniProfilViewModel = ZborDataStandard.ViewModels.ZborViewModels.JavniProfilViewModel;
using ZborDataStandard.ViewModels.RepozitorijViewModels;
using System.IO;
using Microsoft.AspNetCore.SignalR;

namespace ZborApp.Controllers
{
    [Authorize(AuthenticationSchemes =
    JwtBearerDefaults.AuthenticationScheme)]
    public class ApiController : Controller
    {
        private readonly AppSettings appData = new AppSettings();
        private const string LOCATION = "E:/UploadZbor/";
        private const string LOCATION_CHOIR = "E:/UploadZbor/Zbor/";
        private readonly ILogger<KorisnikController> _logger;
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private IUserService _userService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ApiController(ILogger<KorisnikController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager, IUserService userService, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
            _userService = userService;
            _emailSender = new EmailSender();
            _hubContext = hubContext;
        }
        internal class UserID
        {
            public Guid Id { get; set; }
        }
        private UserID GetUser()
        {
            var user = new UserID { Id = Guid.Parse(User.Claims.First(i => i.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value) };
            return user;
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
            var user = GetUser();
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
            var user = GetUser();
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
        public async Task<IActionResult> LajkObavijesti([FromBody] LajkModel lajk)
        {
            var user = GetUser();
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
            await _hubContext.Clients.User(obavijest.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            foreach (var clan in _ctx.ClanZbora.Where(c => c.IdZbor == obavijest.IdZbor).AsEnumerable())
            {
                await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkObavijesti", new { id = idObavijest, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk = true, brojLajkova = brojLajkova + 1 }); ;
            }
            _ctx.LajkObavijesti.Add(l);
            _ctx.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UnlajkObavijesti([FromBody] LajkModel lajk)
        {
            var user = GetUser();
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
                    await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("LajkObavijesti", new { id = idObavijest, jesamja = user.Id == clan.IdKorisnik ? true : false, lajk = false, brojLajkova = brojLajkova - 1 }); ;
                }
                _ctx.LajkObavijesti.Remove(l);
                _ctx.SaveChanges();
            }
            return Ok();

        }
        [HttpPost]
        public async Task<IActionResult> LajkKomentara([FromBody] LajkModel lajk)
        {
            var user = GetUser();
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
            var user = GetUser();
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
            var user = GetUser();
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
            return Ok(k);
        }
        [HttpPost]
        public async Task<IActionResult> NovaObavijest(Guid id, [FromBody]ProfilViewModel model)
        {
            if (!Exists(id))
                return NotFound();
            var user = GetUser();
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
                    await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });


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
            var user = GetUser();
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
            var user = GetUser();
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
        public async Task<IActionResult> NovoPitanje(Guid id, [FromBody] Anketa pitanje)
        {
            var user = GetUser();
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
                await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }

            _ctx.SaveChanges();
            pitanje.IdKorisnikNavigation = _ctx.Korisnik.Find(user.Id);
            return Ok(pitanje);
        }
        [HttpGet]
        public IActionResult Administracija(Guid id)
        {
            if (!Exists(id))
                return NotFound();
            var user = GetUser();
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
        public async Task<IActionResult> PrihvatiPrijavu([FromBody] StringModel model)
        {
            var user = GetUser();
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
            await _hubContext.Clients.User(prijava.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            clan.IdKorisnikNavigation = _ctx.Korisnik.Find(clan.IdKorisnik);
            return Ok(clan);
        }
        [HttpPost]
        public IActionResult PromjenaGlasa([FromBody] PrijavaModel model)
        {
            var user = GetUser();
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
        public async Task<IActionResult> OdbijPrijavu([FromBody] StringModel model)
        {
            var user = GetUser();
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
            var user = GetUser();
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
            await _hubContext.Clients.User(idMod.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            return Ok(mod);
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiModeratora([FromBody] StringModel model)
        {
            var user = GetUser();
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
     
            var m = new StringModel { Value = "ok" };
            return Ok(m);
        }
        [HttpPost]
        public async Task<IActionResult> PostaviVoditelja([FromBody]AdministracijaViewModel modell)
        {
            var user = GetUser();
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
                await _hubContext.Clients.User(cl.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            }
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiClana([FromBody]AdministracijaViewModel model)
        {
            var user = GetUser();
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
            await _hubContext.Clients.User(clan.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
            _ctx.SaveChanges();
            return Ok();
        }



        [HttpPost]
        public IActionResult ObrisiPoziv([FromBody] StringModel model)
        {
            var user = GetUser();
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
        public async Task<IActionResult> PozivZaZbor([FromBody] PrijavaModel prijava)
        {
            var user = GetUser();
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
            pr.IdKorisnikNavigation = _ctx.Korisnik.Find(pr.IdKorisnik);
            return Ok(pr);
        }
        [HttpGet]
        public IActionResult Projekti(Guid id)
        {
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            var user = GetUser();
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
        public async Task<IActionResult> Projekti([FromBody] Projekt model)
        {
            var user = GetUser();
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
                    await _hubContext.Clients.User(cl.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });
                }
                _ctx.SaveChanges();
                return Ok(Novi);
            }
            return NoContent();


        }
        [HttpPost]
        public async Task<IActionResult> PrijavaZaProjekt([FromBody] PrijavaModel prijava)
        {
            var user = GetUser();
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
        [HttpPost]
        public IActionResult PovuciPrijavu(Guid id)
        {
            var user = GetUser();
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
            var user = GetUser();
            var kor = _ctx.Korisnik.Find(user.Id);
            var projekt = _ctx.Projekt.Where(p => p.Id == id).Include(p => p.IdVrstePodjeleNavigation)
                .Include(p => p.Dogadjaj).ThenInclude(d => d.NajavaDolaska)
                .SingleOrDefault();
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
        public async Task<IActionResult> ObrisiDogadjaj(Guid id)
        {
            var user = GetUser();
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
            return Ok();
        }
        [HttpPost]
        public IActionResult NajavaDolaska([FromBody] LajkModel lajk)
        {
            var user = GetUser();
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
        public IActionResult ObrisiNajavuDolaska([FromBody] LajkModel lajk)
        {
            var user = GetUser();
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
        public IActionResult VrsteDogadjaja()
        {
            return Ok(_ctx.VrstaDogadjaja.ToList());
        }
        [HttpPost]
        public async Task<IActionResult> NoviDogadjaj(Guid id, [FromBody]Dogadjaj model)
        {
            var user = GetUser();
            var projekt = _ctx.Projekt.Find(model.IdProjekt);
            if (projekt == null)
                return NotFound();
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            if (model.Naziv.Trim().Equals(""))
                ModelState.AddModelError("Naziv", "Naziv je obavezan.");

            if (model.Lokacija.Trim().Equals(""))
                ModelState.AddModelError("Lokacija", "Lokacija je obavezna.");

            if (model.DodatanOpis.Trim().Equals(""))
                ModelState.AddModelError("DodatanOpis", "Opis je obavezan.");
            if (model.DatumIvrijeme > model.DatumIvrijemeKraja)
                ModelState.AddModelError("DatumIvrijeme", "Ispravno upišite datume.");
            if (ModelState.IsValid)
            {
                if (model.Id == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                {
                    var novi = new Dogadjaj
                    {
                        Id = Guid.NewGuid(),
                        DatumIvrijeme = model.DatumIvrijeme,
                        DatumIvrijemeKraja = model.DatumIvrijemeKraja,
                        DodatanOpis = model.DodatanOpis,
                        IdVrsteDogadjaja = model.IdVrsteDogadjaja,
                        Lokacija = model.Lokacija,
                        Naziv = model.Naziv,
                        IdProjekt = id
                    };
                    _ctx.Dogadjaj.Add(novi);

                }
                else
                {
                    var dog = _ctx.Dogadjaj.Find(model.Id);
                    dog.Naziv = model.Naziv;
                    dog.Lokacija = model.Lokacija;
                    dog.DodatanOpis = model.DodatanOpis;
                    dog.DatumIvrijeme = model.DatumIvrijeme;
                    dog.DatumIvrijemeKraja = model.DatumIvrijemeKraja;
                }
                var pretplatnici = _ctx.PretplataNaProjekt.Where(c => c.IdProjekt == id && c.Dogadjaji).ToHashSet();
                foreach (var pret in pretplatnici)
                {
                    OsobneObavijesti ob = new OsobneObavijesti
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = pret.IdKorisnik,
                        Tekst = String.Format("Novi događaj u projektu <b>{0}</b>.", projekt.Naziv),
                        Procitano = false,
                        Poveznica = "/Zbor/Projekt/" + id
                    };
                    _ctx.Add(ob);
                    await _hubContext.Clients.User(pret.IdKorisnik.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

                }
                _ctx.SaveChanges();
                return Ok();
            }
            //model.IdProjekt = model.Novi.IdProjekt;
            //model.VrsteDogadjaja = _ctx.VrstaDogadjaja.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Naziv }).ToList();
            return NoContent();
        }
        [HttpGet]
        public IActionResult Dogadjaj(Guid id)
        {
            var dog = _ctx.Dogadjaj.Where(d => d.Id == id)
                .Include(d => d.NajavaDolaska).ThenInclude(n => n.IdKorisnikNavigation)
                .Include(d => d.IdProjektNavigation).ThenInclude(p => p.IdVrstePodjeleNavigation).SingleOrDefault();
            if (dog == null)
                return NotFound();    
            var user = GetUser();
            if (!CheckRights(dog.IdProjektNavigation.IdZbor, user.Id))
                return Forbid();
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
            return Ok(model);
        }
        [HttpPost]
        public IActionResult Evidentiraj([FromBody]DogadjajViewModel model)
        {
            var dog = _ctx.Dogadjaj.Where(d => d.Id == model.IdDogadjaj).Include(d => d.IdProjektNavigation).SingleOrDefault();
            if (dog == null)
                return NotFound();
            if (dog.IdProjektNavigation.Zavrsen)
                return NotFound();
            var user = GetUser();
            if (!IsAdmin(dog.IdProjektNavigation.IdZbor, user.Id))
                return Forbid();
            var stare = _ctx.EvidencijaDolaska.Where(d => d.IdDogadjaj == model.IdDogadjaj).ToList();
            _ctx.EvidencijaDolaska.RemoveRange(stare);
            var nove = model.Evidencija.Select(e => new EvidencijaDolaska { Id = Guid.NewGuid(), IdDogadjaj = model.IdDogadjaj, IdKorisnik = e }).ToList();
            _ctx.EvidencijaDolaska.AddRange(nove);
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpGet]
        public IActionResult AdministracijaProjekta(Guid id)
        {

            AdministracijaProjektaViewModel model = new AdministracijaProjektaViewModel();
            var projekt = _ctx.Projekt.Where(z => z.Id == id)
                .Include(z => z.PozivZaProjekt).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.PrijavaZaProjekt).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.ClanNaProjektu).ThenInclude(c => c.IdKorisnikNavigation)
                .Include(z => z.IdVrstePodjeleNavigation)
                .SingleOrDefault();
            if (projekt == null)
                return NotFound();
            var user = GetUser();
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return Forbid();

            model.Projekt = projekt;
            foreach (var glas in projekt.IdVrstePodjeleNavigation.Glasovi())
            {
                model.Clanovi[glas] = new List<ClanNaProjektu>();
            }
            foreach (var clan in projekt.ClanNaProjektu)
            {
                if (!clan.Uloga.Equals("Nema"))
                {

                    model.Clanovi[clan.Uloga].Add(clan);
                }
                else
                    model.Nerazvrstani.Add(clan);
            }
            var zbor = _ctx.Zbor.Find(projekt.IdZbor);
     
            return Ok(model);
        }
        [HttpPost]
        public async Task<IActionResult> PrihvatiPrijavuProjekt([FromBody] StringModel model)
        {
            var user = GetUser();

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
            return Ok(clan);
        }
        [HttpPost]
        public async Task<IActionResult> OdbijPrijavuProjekt([FromBody] StringModel model)
        {
            var user = GetUser();
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
        public async Task<IActionResult> DodajClanProjekt([FromBody] LajkModel model)
        {
            var user = GetUser();
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
            var clan = _ctx.ClanNaProjektu.Where(p => p.IdKorisnik == id && p.IdProjekt == idProjekt).Include(p => p.IdProjektNavigation).SingleOrDefault();

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
            return Ok(c);
        }

        [HttpPost]
        public IActionResult PretragaKorisnikaProjekt([FromBody] PretragaModel model)
        {
            Guid idProjekt;
            var flag = Guid.TryParse(model.Id, out idProjekt);
            if (!flag)
                return BadRequest();
            var projekt = _ctx.Projekt.Find(idProjekt);
            if (projekt == null)
                return NotFound();
            var user = GetUser();
            if (!CheckRights(projekt.IdZbor, user.Id))
                return Forbid();
            var korisnici = _ctx.Korisnik.Where(k => (k.Ime.Trim().ToLower() + ' ' + k.Prezime.Trim().ToLower()).Contains(model.Tekst))
                .Where(k => k.ClanZbora.Where(c => c.IdZbor == projekt.IdZbor && c.IdKorisnik == k.Id).SingleOrDefault() != null)
                .Where(k => k.ClanNaProjektu.Where(c => c.IdProjekt == projekt.Id && c.IdKorisnik == k.Id).SingleOrDefault() == null && k.PozivZaProjekt.Where(c => c.IdProjekt == projekt.Id && c.IdKorisnik == k.Id).SingleOrDefault() == null && k.PrijavaZaProjekt.Where(c => c.IdProjekt == projekt.Id && c.IdKorisnik == k.Id).SingleOrDefault() == null)
                .ToList();

            return Ok(korisnici);
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiClanaProjekta([FromBody]AdministracijaProjektaViewModel model)
        {
            var user = GetUser();
            var clan = _ctx.ClanNaProjektu.Where(c => c.Id == model.IdBrisanje).Include(c => c.IdProjektNavigation).SingleOrDefault();
            if (clan == null)
                return NotFound();
            if (!IsAdmin(clan.IdProjektNavigation.IdZbor, user.Id))
                return Forbid();
            if (clan.IdProjektNavigation.Zavrsen)
                return NotFound();
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
            return Ok();
        }
        [HttpPost]
        public IActionResult PromjenaUloge([FromBody] PrijavaModel model)
        {
            var user = GetUser();
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
        public IActionResult DohvatiStatistiku([FromBody] LajkModel model)
        {
            Guid idKorisnik, idProjekt;
            var flagK = Guid.TryParse(model.IdKorisnik, out idKorisnik);
            var flagP = Guid.TryParse(model.IdCilj, out idProjekt);
            if (flagK == false || flagP == false)
                return BadRequest();
            var dogadaji = _ctx.Dogadjaj.Where(d => d.IdProjekt == idProjekt)
                .Include(d => d.EvidencijaDolaska).OrderByDescending(d => d.DatumIvrijeme);
            var ev = dogadaji.Where(d => d.EvidencijaDolaska.Select(e => e.IdKorisnik).Contains(idKorisnik)).ToList();
            double postotak = 0.0;
            if (dogadaji.Count() > 0)
                postotak = 1.0 * ev.Count / (dogadaji.Count()) * 100;
            var response = new
            {
                Evidentirani = ev,
                Neevidentirani = dogadaji.Where(d => !d.EvidencijaDolaska.Select(e => e.IdKorisnik).Contains(idKorisnik)).ToList(),
                Postotak = postotak
            };
            return Ok(response);
        }
        [HttpPost]
        public IActionResult ObrisiProjekt([FromBody]AdministracijaProjektaViewModel model)
        {
            var user = GetUser();
            var projekt = _ctx.Projekt.Find(model.IdBrisanje);
            if (projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            _ctx.Remove(projekt);
            _ctx.SaveChanges();

            return Ok();
        }
        [HttpPost]
        public IActionResult ZavrsiProjekt([FromBody] AdministracijaProjektaViewModel model)
        {
            var user = GetUser();
            var projekt = _ctx.Projekt.Find(model.IdBrisanje);
            if (projekt == null)
                return RedirectToAction("Nema", "Greska");
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            projekt.Zavrsen = true;
            _ctx.SaveChanges();

            return Ok();
        }
        [HttpGet]
        public IActionResult Pretplate(Guid id)
        {
            var user = GetUser();
            if (!Exists(id))
                return NotFound();
            if (!CheckRights(id, user.Id))
                return Forbid();
            var zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.PretplataNaZbor).Include(z => z.Projekt).ThenInclude(p => p.PretplataNaProjekt).SingleOrDefault();

            PretplateViewModel model = new PretplateViewModel
            {
                Zbor = zbor,
                Projekti = zbor.Projekt.ToList(),
                PretplataZbor = zbor.PretplataNaZbor.Where(p => p.IdKorisnik == user.Id).SingleOrDefault(),
                PretplataProjekt = zbor.Projekt.Select(p => p.PretplataNaProjekt.Where(p => p.IdKorisnik == user.Id).FirstOrDefault()).ToList()
            };
            return Ok(model);
        }

        [HttpPost]
        public IActionResult Pretplate(Guid id, [FromBody]PretplateViewModel model)
        {
            var user = GetUser();
            if (!Exists(id))
                return NotFound();
            if (!CheckRights(id, user.Id))
                return Forbid();
            var pretplataZbor = _ctx.PretplataNaZbor.Where(z => z.IdZbor == id && z.IdKorisnik == user.Id).SingleOrDefault();
            if (pretplataZbor == null)
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
            foreach (var pretplata in model.PretplataProjekt)
            {
                var pretplataProjekt = _ctx.PretplataNaProjekt.Where(z => z.IdProjekt == pretplata.Id && z.IdKorisnik == user.Id).SingleOrDefault();
                if (pretplataProjekt == null)
                {
                    pretplataProjekt = new PretplataNaProjekt
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = user.Id,
                        IdProjekt = pretplata.IdProjekt,
                        Obavijesti = pretplata.Obavijesti,
                        Dogadjaji = pretplata.Dogadjaji
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


            return Ok();
        }
        [HttpGet]
        public IActionResult JavniProfil(Guid id)
        {
            var user = GetUser();
            if (!Exists(id))
                return NotFound();
            var zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.ProfilZbor)
                .Include(z => z.Voditelj).ThenInclude(v => v.IdKorisnikNavigation).SingleOrDefault();

            ZborDataStandard.ViewModels.ZborViewModels.JavniProfilViewModel model = new ZborDataStandard.ViewModels.ZborViewModels.JavniProfilViewModel
            {
                Zbor = zbor,
                Mod = IsAdmin(id, user.Id),
                Clan = CheckRights(id, user.Id)
            };
          
            return Ok(model);
        }
        [HttpPost]
        public IActionResult Urediozboru([FromBody] PretragaModel model)
        {
            var user = GetUser();
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
        public IActionResult Urediovoditeljima([FromBody] PretragaModel model)
        {
            var user = GetUser();
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
        public IActionResult Uredirepertoar([FromBody] PretragaModel model)
        {
            var user = GetUser();
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
        public IActionResult Uredireprezentacija([FromBody] PretragaModel model)
        {
            var user = GetUser();
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
        public IActionResult ZborRepozitorij(Guid id)
        {
            var user = GetUser();
            var korisnik = _ctx.Korisnik.Where(k => k.Id == user.Id).SingleOrDefault();
            var datoteke = _ctx.RepozitorijZbor.Where(r => r.IdZbor == id).Include(k => k.IdKorisnikNavigation).OrderByDescending(r => r.DatumPostavljanja).ToList();
            if (!_ctx.ClanZbora.Where(c => c.IdZbor == id).Select(c => c.IdKorisnik).Contains(user.Id))
                datoteke = datoteke.Where(d => d.Privatno == false).ToList();

            var zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.Voditelj).Include(z => z.ModeratorZbora).SingleOrDefault();
            var admin = zbor.Voditelj.OrderByDescending(z => z.DatumPostanka).First();
            var mod = zbor.ModeratorZbora.Where(m => m.IdKorisnik == user.Id).SingleOrDefault();
            bool flagAdmin = admin.IdKorisnik == user.Id || mod != null ? true : false;
            var clan = _ctx.ClanZbora.Where(c => c.IdKorisnik == user.Id && c.IdZbor == id).SingleOrDefault();
            bool flagClan = clan == null ? false : true;
            RepozitorijZborViewModel model = new RepozitorijZborViewModel
            {
                Datoteke = datoteke,
                IdKorisnik = user.Id,
                IdTrazeni = id,
                IdZbor = id,
                Promjena = flagAdmin,
                Clan = flagClan
            };
            return Ok(model);
        }
        public IActionResult DeleteZbor([FromBody]RepozitorijZborViewModel model)
        {
            var user = GetUser();
            var dat = _ctx.RepozitorijZbor.Find(model.IdTrazeni);
            if (dat == null)
                return NotFound();
            if (!IsAdmin(dat.IdZbor, user.Id) && dat.IdKorisnik != user.Id)
                return Forbid();
            try
            {

                System.IO.File.Delete(LOCATION + "/" + dat.Url);
                _ctx.Remove(dat);
                _ctx.SaveChanges();
            }
            catch (Exception)
            {
                return NotFound();
            };


            return Ok();
        }
        [HttpPost]
        public IActionResult ObjaviZbor([FromBody] StringModel model)
        {
            var user = GetUser();
            Guid idRep;
            var flag = Guid.TryParse(model.Value, out idRep);
            if (flag == false)
                return BadRequest();
            var d = _ctx.RepozitorijZbor.Find(idRep);
            if (d == null)
                return NotFound();
            if (!IsAdmin(d.IdZbor, user.Id) && d.IdKorisnik != user.Id)
                return Forbid();
            d.Privatno = false;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpPost]
        public IActionResult PrivatizirajZbor([FromBody] StringModel model)
        {
            var user = GetUser();
            Guid idRep;
            var flag = Guid.TryParse(model.Value, out idRep);
            if (flag == false)
                return BadRequest();
            var d = _ctx.RepozitorijZbor.Find(idRep);
            if (d == null)
                return NotFound();
            if (!IsAdmin(d.IdZbor, user.Id) && d.IdKorisnik != user.Id)
                return Forbid();
            d.Privatno = true;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [AllowAnonymous]
        public IActionResult GetRepozitorijZbor(Guid id)
        {
            var dat = _ctx.RepozitorijZbor.Where(d => d.Id == id).SingleOrDefault();
            if (dat == null)
                return NotFound();
            return File(System.IO.File.ReadAllBytes(LOCATION + "/" + dat.Url), "application/force-download", dat.Naziv);
        }
        [AllowAnonymous]
        public IActionResult GetRepozitorijKorisnik(Guid id)
        {
            var dat = _ctx.RepozitorijKorisnik.Where(d => d.Id == id).SingleOrDefault();
            if (dat == null)
                return NotFound();
            return File(System.IO.File.ReadAllBytes(LOCATION + "/" + dat.Url), "application/force-download", dat.Naziv);

        }
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadRepozitorijZbor(Guid id, [FromForm(Name ="file")]IFormFile formFile)
        {
            var user = GetUser();

            RepozitorijZbor dat = null;
                if (formFile.Length > 0)
                {
                    DirectoryInfo di = Directory.CreateDirectory(LOCATION_CHOIR + id);
                    var filePath = "Zbor/" + id + "/" + formFile.FileName;
                    dat = new RepozitorijZbor
                    {
                        Id = Guid.NewGuid(),
                        DatumPostavljanja = DateTime.Now,
                        IdKorisnik = user.Id,
                        IdZbor = id,
                        Url = filePath,
                        Privatno = true,
                        Naziv = formFile.FileName

                    };

                    var fullPath = LOCATION + filePath;

                    using (var stream = System.IO.File.Create(fullPath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    _ctx.Add(dat);

                }
                var pretplatnici = _ctx.PretplataNaZbor.Where(p => p.IdZbor == id).Select(p => p.IdKorisnik).ToHashSet();
                foreach (var pret in pretplatnici)
                {
                    OsobneObavijesti ob = new OsobneObavijesti
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = pret,
                        Tekst = String.Format("Nove datoteke u zboru <b>{0}</b>.", _ctx.Zbor.Find(id).Naziv),
                        Procitano = false,
                        Poveznica = "/Repozitorij/Zbor/" + id
                    };
                    _ctx.Add(ob);
                    await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

                }
                _ctx.SaveChanges();
            

            return Ok(dat);
        }

        public IActionResult KorisnikRepozitorij(Guid id)
        {
            var user = GetUser();
            var korisnik = _ctx.Korisnik.Where(k => k.Id == user.Id).SingleOrDefault();
            var datoteke = _ctx.RepozitorijKorisnik.Where(r => r.IdKorisnik == id).OrderByDescending(r => r.DatumPostavljanja).ToList();
            if (id != korisnik.Id)
                datoteke = datoteke.Where(d => d.Privatno == false).ToList();

            RepozitorijViewModel model = new RepozitorijViewModel
            {
                Datoteke = datoteke,
                IdKorisnik = user.Id,
                IdTrazeni = id
            };
            return Ok(model);
        }
        public IActionResult ObrisiKorisnikRep(RepozitorijViewModel model)
        {
            var user = GetUser();

            var d = _ctx.RepozitorijKorisnik.Find(model.IdTrazeni);
            if (d == null)
                return RedirectToAction("Nema", "Greska");
            if (d.IdKorisnik != user.Id)
                return RedirectToAction("Prava", "Zbor");
            try
            {

                System.IO.File.Delete(LOCATION + "/" + d.Url);
                _ctx.Remove(d);
                _ctx.SaveChanges();
            }
            catch (Exception)
            {
                return NotFound() ;
            };


            return Ok();
        }
        [HttpPost]
        public IActionResult ObjaviKorisnik([FromBody] StringModel model)
        {
            var user = GetUser();
            Guid idRep;
            var flag = Guid.TryParse(model.Value, out idRep);
            if (flag == false)
                return BadRequest();
            //provjera usera bla bla
            var d = _ctx.RepozitorijKorisnik.Find(idRep);
            if (d == null)
                return NotFound();
            if (d.IdKorisnik != user.Id)
                return Forbid();
            d.Privatno = false;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }

        [HttpPost]
        public IActionResult PrivatizirajKorisnik([FromBody] StringModel model)
        {
            var user = GetUser();
            Guid idRep;
            var flag = Guid.TryParse(model.Value, out idRep);
            if (flag == false)
                return BadRequest();
            //provjera usera bla bla
            var d = _ctx.RepozitorijKorisnik.Find(idRep);
            if (d == null)
                return NotFound();
            if (d.IdKorisnik != user.Id)
                return Forbid();
            d.Privatno = true;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [AllowAnonymous]
        public IActionResult GetKorisnikRep(Guid id)
        {
            var dat = _ctx.RepozitorijKorisnik.Where(d => d.Id == id).SingleOrDefault();
            if (dat == null)
                return NotFound();
            return File(System.IO.File.ReadAllBytes(LOCATION + "/" + dat.Url), "application/force-download", dat.Naziv);

        }
        public async Task<IActionResult> UploadKorisnik(RepozitorijViewModel model)
        {
            var user = GetUser();
            var files = model.FormFiles;
            long size = files.Sum(f => f.Length);
            RepozitorijKorisnik dat = null;
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    DirectoryInfo di = Directory.CreateDirectory(LOCATION + "/" + user.Id);
                    var filePath = user.Id + "/" + formFile.FileName;
                    dat = new RepozitorijKorisnik
                    {
                        Id = Guid.NewGuid(),
                        DatumPostavljanja = DateTime.Now,
                        IdKorisnik = user.Id,
                        Url = filePath,
                        Privatno = true,
                        Naziv = formFile.FileName

                    };

                    var fullPath = LOCATION + filePath;

                    using (var stream = System.IO.File.Create(fullPath))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    _ctx.Add(dat);

                }
                _ctx.SaveChanges();
            }

            return Ok(dat);
        }
        public IActionResult RazgovoriKorisnik()
        {
            var user = GetUser();
            var korisnik = _ctx.Korisnik.Where(k => k.Id == user.Id).SingleOrDefault();
            var razg = _ctx.Razgovor.Where(r => r.KorisnikUrazgovoru.Select(kr => kr.IdKorisnik).Contains(korisnik.Id))
                .Include(r => r.KorisnikUrazgovoru).ThenInclude(kr => kr.IdKorisnikNavigation)
                .Include(r => r.Poruka).ThenInclude(p => p.IdKorisnikNavigation).OrderByDescending(r => r.DatumZadnjePoruke).ToList();
            foreach(var r in razg)
            {
                r.Poruka = r.Poruka.OrderByDescending(p => p.DatumIvrijeme).ToList();
                var list = new List<Poruka>();
                list.Add(r.Poruka.First());
                r.Poruka = list;
            }
            ZborDataStandard.ViewModels.PorukeViewModels.PorukeViewModel model = new ZborDataStandard.ViewModels.PorukeViewModels.PorukeViewModel
            {
                Razgovori = razg
             
            };
            return Ok(model);
        }
        public IActionResult Poruke(Guid id)
        {
            var user = GetUser();
            var korisnik = _ctx.Korisnik.Where(k => k.Id == user.Id).SingleOrDefault();
            var razg = _ctx.Razgovor.Where(r => r.Id == id )
                .Include(r => r.KorisnikUrazgovoru).ThenInclude(kr => kr.IdKorisnikNavigation)
                .Include(r => r.Poruka).ThenInclude(p => p.IdKorisnikNavigation).OrderByDescending(r => r.DatumZadnjePoruke).SingleOrDefault();
            if (razg == null)
                return NotFound();
            if (!razg.KorisnikUrazgovoru.Select(k => k.IdKorisnik).Contains(user.Id))
                return Forbid();
           
            razg.Poruka = razg.Poruka.OrderBy(p => p.DatumIvrijeme).ToList();
        
            
            
            return Ok(razg.Poruka.ToList());
        }
        public IActionResult Korisnici()
        {
            var user = GetUser();
            var korisnici = _ctx.Korisnik.Where(k=> k.Id != user.Id).ToList();
            return Ok(korisnici);
        }
        [HttpGet]
        public async Task<IActionResult> ObavijestiOsobne()
        {
            var user = GetUser();
            var model = new ObavijestiViewModel { Obavijesti = _ctx.OsobneObavijesti.Where(r => r.IdKorisnik == user.Id).OrderByDescending(r => r.Datum).AsNoTracking().ToList() };
            await _ctx.OsobneObavijesti.Where(o => o.IdKorisnik == user.Id && o.Procitano == false).ForEachAsync(o => o.Procitano = true);
            _ctx.SaveChanges();

            return Ok(model);
        }
        public IActionResult GalerijaZbor(Guid id)
        {
            var user = GetUser();
            if (!Exists(id))
                return RedirectToAction("Nema", "Greska");
            var slike = _ctx.RepozitorijZbor.Where(z => z.IdZbor == id).ToList().Where(s => s.JeSlika()).ToList();
            var clan = CheckRights(id, user.Id);
            if (clan == false)
            {
                slike = slike.Where(s => s.Privatno == false).ToList();
            }
            ZborDataStandard.ViewModels.ZborViewModels.GalerijaViewModel model = new ZborDataStandard.ViewModels.ZborViewModels.GalerijaViewModel
            {
                Slike = slike,
                Clan = clan,
                IdZbor = id
            };
            
            return Ok(model);
        }
        public IActionResult PromjenaProfilneZbor(Guid id)
        {
            var user = GetUser();
            var dat = _ctx.RepozitorijZbor.Find(id);
            if (dat == null)
                return NotFound();
            if (!CheckRights(dat.IdZbor, user.Id))
                return Forbid();
            dat.Privatno = false;
            _ctx.Zbor.Find(dat.IdZbor).IdSlika = id;
            _ctx.SaveChanges();
            return Ok();
        }
        public IActionResult GalerijaKorisnik(Guid id)
        {
            var user = GetUser();
            var korisnik = _ctx.Korisnik.Find(id);
            if (korisnik == null)
                return NotFound();
            var slike = _ctx.RepozitorijKorisnik.Where(z => z.IdKorisnik == id).ToList().Where(s => s.JeSlika()).ToList();
            if (user.Id != id)
            {
                slike = slike.Where(s => s.Privatno == false).ToList();
            }
            ZborDataStandard.ViewModels.KorisnikVIewModels.GalerijaViewModel model = new ZborDataStandard.ViewModels.KorisnikVIewModels.GalerijaViewModel
            {
                Slike = slike,
                Clan = user.Id == id,
                IdKorisnik = id
            };
            return Ok(model);
        }
        public IActionResult PromjenaProfilneKorisnik(Guid id)
        {
            var user = GetUser();
            var dat = _ctx.RepozitorijKorisnik.Find(id);
            if (dat == null)
                return NotFound();
            if (user.Id != dat.IdKorisnik)
                return Forbid();
            dat.Privatno = false;
            _ctx.Korisnik.Find(user.Id).IdSlika = id;
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> PrijavaZbor(Guid id)
        {
            var user = GetUser();
            if (!Exists(id))
                return NotFound();
            var prijavaZaZbor = _ctx.PrijavaZaZbor.Where(p => p.IdKorisnik == user.Id && p.IdZbor == id).SingleOrDefault();
            if (prijavaZaZbor != null)
            {
                _ctx.Remove(prijavaZaZbor);
                _ctx.SaveChanges();
                return Ok();
            }

            var clan = _ctx.ClanZbora.Where(p => p.IdKorisnik == user.Id && p.IdZbor == id).SingleOrDefault();
            if (clan != null)
                return Ok();
            var pr = new PrijavaZaZbor
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdZbor = id,
                Poruka = "",
                DatumPrijave = DateTime.Now
            };
            _ctx.PrijavaZaZbor.Add(pr);
            var pretplatnici = _ctx.ModeratorZbora.Where(p => p.IdZbor == id).Select(p => p.IdKorisnik).ToHashSet();
            pretplatnici.Add(_ctx.Voditelj.Where(v => v.IdZbor == id).Select(p => p.IdKorisnik).FirstOrDefault());
            foreach (var pret in pretplatnici)
            {
                OsobneObavijesti ob = new OsobneObavijesti
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = pret,
                    Tekst = String.Format("<b>{0}</b> se prijavljuje za zbor <b>{1}</b>.", _ctx.Korisnik.Find(user.Id).ImeIPrezime(), _ctx.Zbor.Find(id).Naziv),
                    Procitano = false,
                    Poveznica = "/Zbor/Administracija/" + id
                };
                _ctx.Add(ob);
                await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

            }


            _ctx.SaveChanges();
            return Ok();
        }
        [HttpGet]
        public IActionResult JavniProfilKorisnik(Guid id)
        {
            var user = GetUser();
            var korisnik = _ctx.Korisnik.Where(k => k.Id == id).Include(k => k.ClanZbora).ThenInclude(c => c.IdZborNavigation).SingleOrDefault();
            if (korisnik == null)
                return NotFound();

            ZborDataStandard.ViewModels.KorisnikVIewModels.JavniProfilViewModel model = new ZborDataStandard.ViewModels.KorisnikVIewModels.JavniProfilViewModel
            {
                Korisnik = korisnik,
                Aktivni = user.Id
            };

            return Ok(model);
        }
        [HttpPost]
        public IActionResult Urediomeni([FromBody] PretragaModel model)
        {
            var user = GetUser();
            _ctx.Korisnik.Find(user.Id).Opis = model.Tekst;
            _ctx.SaveChanges();
            return Ok();
        }
    }
}
