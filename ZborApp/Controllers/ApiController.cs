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
    }
}
