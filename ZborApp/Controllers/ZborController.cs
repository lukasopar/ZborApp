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
        public ZborController(ILogger<ZborController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
            _emailSender = new EmailSender();
        }
        private bool CheckRights(Guid idZbor, Guid idKorisnik)
        {
            var clan = _ctx.ClanZbora.Where(c => c.IdKorisnik == idKorisnik && c.IdZbor == idZbor).SingleOrDefault();
            var voditelj = _ctx.Voditelj.Where(v => v.IdZbor == idZbor).OrderByDescending(v => v.DatumPostanka).SingleOrDefault();
            if (clan != null || voditelj.IdKorisnik == idKorisnik)
                return true;
            return false;
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
                model.Novi.Voditelj.Add(voditelj);
                _ctx.Add(model.Novi);
                _ctx.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> PrijavaZaZbor([FromBody] PrijavaModel prijava)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var idZbor = Guid.Parse(prijava.Id);
            var prijavaZaZbor = _ctx.PrijavaZaZbor.Where(p => p.IdKorisnik == user.Id && p.IdZbor == idZbor).SingleOrDefault();
            if (prijavaZaZbor != null)
                return Ok();
            var pr = new PrijavaZaZbor
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdZbor = idZbor,
                Poruka = prijava.Poruka,
                DatumPrijave = DateTime.Now
            };
            _ctx.PrijavaZaZbor.Add(pr);
            _ctx.SaveChanges();
            return Ok();
        }
        private bool IsAdmin(Guid idZbor, Guid idKorisnik)
        {
            var zbor = _ctx.Zbor.Where(z => z.Id == idZbor).Include(z => z.Voditelj).SingleOrDefault();
            var admin = zbor.Voditelj.OrderByDescending(z => z.DatumPostanka).First();
            return admin.IdKorisnik == idKorisnik ? true : false;
        }
        [HttpGet]
        public async Task<IActionResult> Profil(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
            var korisnik = _ctx.Korisnik.Where(k => k.Id == user.Id).SingleOrDefault();
            Zbor zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.Voditelj).SingleOrDefault();
            if (zbor == null)
                return Error();
            IEnumerable<Obavijest> obavijesti = _ctx.Obavijest.Where(o => o.IdZbor == id)
                .Include(o => o.IdKorisnikNavigation)
                .Include(o => o.LajkObavijesti)
                .Include(o => o.KomentarObavijesti).ThenInclude(k => k.LajkKomentara)
                .Include(o => o.KomentarObavijesti).ThenInclude(k => k.IdKorisnikNavigation)
                  .Include(o => o.KomentarObavijesti).OrderBy(d => d.DatumObjave)
                .OrderByDescending(O => O.DatumObjave);
            var ankete = _ctx.Anketa.Where(a => a.IdZbor == id).OrderByDescending(a => a.DatumPostavljanja);
            ProfilViewModel model = new ProfilViewModel { Zbor = zbor, Obavijesti = obavijesti, IdKorisnik = user.Id, ImeIPrezime = korisnik.Ime + " " + korisnik.Prezime, Slika = korisnik.Slika };
            var admin = zbor.Voditelj.OrderByDescending(z => z.DatumPostanka).First();
            model.Admin = IsAdmin(id, user.Id);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Pitanja(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
            var aktivna = _ctx.Anketa.Where(a => a.IdZbor == id && a.DatumKraja >= DateTime.Now).OrderBy(a => a.DatumKraja);
            var prosla = _ctx.Anketa.Where(a => a.IdZbor == id && a.DatumKraja < DateTime.Now).OrderByDescending(a => a.DatumKraja);
            PitanjaViewModel model = new PitanjaViewModel
            {
                Admin = IsAdmin(id, user.Id),
                AktivnaPitanja = aktivna,
                GotovaPitanja = prosla,
                IdZbor = id
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profil(Guid id, ProfilViewModel model)
        {
            model.Zbor = _ctx.Zbor.Where(z => z.Id == id).SingleOrDefault();
            if(ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
                model.NovaObavijest.DatumObjave = DateTime.Now;
                model.NovaObavijest.Id = Guid.NewGuid();
                model.NovaObavijest.IdZbor = id;
                model.NovaObavijest.IdKorisnik = user.Id;
                _ctx.Add(model.NovaObavijest);
                _ctx.SaveChanges();
                return RedirectToAction("Profil", new { id = id });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> LajkObavijesti([FromBody] LajkModel lajk)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var l = new LajkObavijesti
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdObavijest = Guid.Parse(lajk.IdCilj)

            };
            _ctx.LajkObavijesti.Add(l);
            _ctx.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> LajkKomentara([FromBody] LajkModel lajk)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var l = new LajkKomentara
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdKomentar = Guid.Parse(lajk.IdCilj)

            };
            _ctx.LajkKomentara.Add(l);
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> NoviKomentar([FromBody] NoviKomentarModel komentar)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var k = new KomentarObavijesti
            {
                DatumObjave = DateTime.Now,
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdObavijest = Guid.Parse(komentar.IdObavijest),
                Tekst = komentar.Tekst
            };
            _ctx.KomentarObavijesti.Add(k);
            _ctx.SaveChanges();
            return Ok(k);
        }

        [HttpGet]
        public async Task<IActionResult> NovaAnketa(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
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
                Pitanje = anketaJSON.pitanje,
                DatumKraja = model.DatumKraj,
                VisestrukiOdgovor = anketaJSON.vrsta.Equals("one") ? false : true
                
            };
            int i = 0;
            foreach(string odgovor in anketaJSON.odgovori)
            {
                OdgovorAnkete odg = new OdgovorAnkete
                {
                    Id = Guid.NewGuid(),
                    IdAnketa = anketa.Id,
                    Redoslijed = i,
                    Odgovor = odgovor,
                    
                };
                anketa.OdgovorAnkete.Add(odg);
                i++;
            }
            _ctx.Anketa.Add(anketa);
            _ctx.SaveChanges();
            return RedirectToAction("Profil", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> RijesiAnketu(Guid idZbor, Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(idZbor, user.Id))
                return RedirectToAction("Prava");
            Anketa anketa = _ctx.Anketa.Where(a => a.Id == id).Include(a => a.OdgovorAnkete).ThenInclude(o => o.OdgovorKorisnikaNaAnketu).SingleOrDefault();
           
            anketa.OdgovorAnkete = anketa.OdgovorAnkete.OrderBy(p => p.Redoslijed).ToList();


            //slaganje JSONa iz jedne ankete
            PitanjeJSON anketaJSON = new PitanjeJSON
            {
                pitanje = anketa.Pitanje,
                odgovori = new List<string>()
            };

            List<int> odg = new List<int>();
            foreach (var odgovor in anketa.OdgovorAnkete)
            {
                anketaJSON.odgovori.Add(odgovor.Odgovor);
                OdgovorKorisnikaNaAnketu odgovorNaPitanje = _ctx.OdgovorKorisnikaNaAnketu.Where(o => o.IdOdgovor == odgovor.Id && o.IdKorisnik == user.Id).Include(o => o.IdOdgovorNavigation).FirstOrDefault();
                if (odgovorNaPitanje != null)
                    odg.Add(odgovorNaPitanje.IdOdgovorNavigation.Redoslijed);
            }
            anketaJSON.vrsta = !anketa.VisestrukiOdgovor ?"one" : "mul";
        
            PrikazAnketeViewModel model = new PrikazAnketeViewModel
            {
                DatumKraja = anketa.DatumKraja.ToString("yyyy-MM-ddTHH:mm:ss"),
                AnketaJson = JsonConvert.SerializeObject(anketaJSON),
                Id = id,
                Odgovor = JsonConvert.SerializeObject(odg),
                Anketa = anketa
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RijesiAnketu(Guid idZbor, Guid id, PrikazAnketeViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var stariOdgovor = _ctx.OdgovorKorisnikaNaAnketu.Where(o => o.IdOdgovorNavigation.IdAnketa == id && o.IdKorisnik == user.Id).ToList();
            if(stariOdgovor != null) 
                _ctx.OdgovorKorisnikaNaAnketu.RemoveRange(stariOdgovor);
            var noviOdgovori = model.Rjesenje.Trim().Split(" ");
            foreach(var odg in noviOdgovori)
            {
                try
                {
                    OdgovorKorisnikaNaAnketu odgovor = new OdgovorKorisnikaNaAnketu
                    {
                        DatumOdgovora = DateTime.Now,
                        Id = Guid.NewGuid(),
                        IdKorisnik = user.Id
                    };
                    odgovor.IdOdgovor = _ctx.OdgovorAnkete.Where(o => o.IdAnketa == id && o.Redoslijed == Int32.Parse(odg)).SingleOrDefault().Id;
                    _ctx.OdgovorKorisnikaNaAnketu.Add(odgovor);
                } catch (Exception ex)
                {
                    throw ex;
                }
            }
            _ctx.SaveChanges();
            return RedirectToAction("Profil", new { id = idZbor });
        }
        [HttpGet]
        public async Task<IActionResult> Administracija(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!IsAdmin(id, user.Id))
                return RedirectToAction("Prava");
            AdministracijaViewModel model = new AdministracijaViewModel();
            var zbor = _ctx.Zbor.Where(z => z.Id == id)
                .Include(z => z.PozivZaZbor).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.PrijavaZaZbor).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.ClanZbora).ThenInclude(c => c.IdKorisnikNavigation)
                .SingleOrDefault();
           
            model.Zbor = zbor;
            model.Soprani = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("sopran")).ToList();
            model.Alti = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("alt")).ToList();
            model.Tenori = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("tenor")).ToList();
            model.Basi = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("bas")).ToList();
            model.Nerazvrstani = zbor.ClanZbora.Where(c => c.Glas.Trim().Equals("ne")).ToList();
            return View(model);
        }
        [HttpPost]
        public IActionResult PrihvatiPrijavu([FromBody] StringModel model)
        {
            PrijavaZaZbor prijava = _ctx.PrijavaZaZbor.Where(p => p.Id == Guid.Parse(model.Value)).Include(p => p.IdKorisnikNavigation).SingleOrDefault();
            ClanZbora clan = new ClanZbora
            {
                Id = Guid.NewGuid(),
                DatumPridruzivanja = DateTime.Now,
                Glas = "Ne",
                IdZbor = prijava.IdZbor,
                IdKorisnik = prijava.IdKorisnik
            };
            _ctx.ClanZbora.Add(clan);
            _ctx.PrijavaZaZbor.Remove(prijava);
            _ctx.SaveChanges();
            var m = new StringModel { Value = prijava.IdKorisnikNavigation.Ime + ' ' + prijava.IdKorisnikNavigation.Prezime };
            return Ok(m);
        }

        [HttpPost]
        public IActionResult OdbijPrijavu([FromBody] StringModel model)
        {
            PrijavaZaZbor prijava = _ctx.PrijavaZaZbor.Where(p => p.Id == Guid.Parse(model.Value)).SingleOrDefault();
            _ctx.PrijavaZaZbor.Remove(prijava);
            _ctx.SaveChanges();
            var m = new StringModel { Value = "OKje" };

            return Ok(m);
        }

        [HttpPost]
        public IActionResult ObrisiKomentar([FromBody] StringModel model)
        {
            KomentarObavijesti kom = _ctx.KomentarObavijesti.Where(p => p.Id == Guid.Parse(model.Value)).Include(k => k.LajkKomentara).SingleOrDefault();
            _ctx.LajkKomentara.RemoveRange(kom.LajkKomentara);
            _ctx.KomentarObavijesti.Remove(kom);
            _ctx.SaveChanges();
            var m = new StringModel { Value = "OKje" };

            return Ok(m);
        }
        public IActionResult ObrisiObavijest(Guid id)
        {
            Obavijest o = _ctx.Obavijest.Where(p => p.Id == id).Include(o => o.KomentarObavijesti).ThenInclude(k => k.LajkKomentara).Include(o => o.LajkObavijesti).SingleOrDefault();
            var idZbor = o.IdZbor;
            _ctx.LajkObavijesti.RemoveRange(o.LajkObavijesti);
            foreach(var k in o.KomentarObavijesti)
            {
                _ctx.LajkKomentara.RemoveRange(k.LajkKomentara);
                _ctx.Remove(k);

            }
            _ctx.Obavijest.Remove(o);
            _ctx.SaveChanges();

            return RedirectToAction("Profil", new { id = idZbor });
        }


        [HttpPost]
        public IActionResult ObrisiPoziv([FromBody] StringModel model)
        {
            PozivZaZbor poziv = _ctx.PozivZaZbor.Where(p => p.Id == Guid.Parse(model.Value)).SingleOrDefault();
            _ctx.PozivZaZbor.Remove(poziv);
            _ctx.SaveChanges();
            var m = new StringModel { Value = "OKje" };

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
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!CheckRights(id, user.Id))
                return RedirectToAction("Prava");
           
            var mojiProjekti = _ctx.Projekt.Where(p => id==p.IdZbor).Where(z => z.ClanNaProjektu.Select(v => v.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var prijaveProjekti = _ctx.Projekt.Where(p => id == p.IdZbor).Where(z => z.PrijavaZaProjekt.Select(p => p.IdKorisnik).Contains(user.Id)).AsEnumerable();
            var ostaliProjekti = _ctx.Projekt.Where(p => id == p.IdZbor).Where(z => !z.ClanNaProjektu.Select(v => v.IdKorisnik).Contains(user.Id) && !z.PrijavaZaProjekt.Select(p => p.IdKorisnik).Contains(user.Id)).AsEnumerable();
            ProjektiViewModel model = new ProjektiViewModel
            {
                IdZbor = id,
                MojiProjekti = mojiProjekti,
                IdKorisnik = user.Id,
                Admin = IsAdmin(id, user.Id),
                OstaliProjekti = ostaliProjekti,
                PrijavaProjekti = prijaveProjekti,
                Projekti = _ctx.Projekt.Where(p => p.IdZbor == id).Include(p => p.ClanNaProjektu).Include(p => p.PrijavaZaProjekt),
                VrstePodjele = _ctx.VrstaPodjele.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Podjela }).ToList()
                
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Projekti(ProjektiViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Novi.Id = Guid.NewGuid();
                _ctx.Add(model.Novi);
                _ctx.SaveChanges();
                return RedirectToAction("Projekti", new { id = model.Novi.IdZbor });
            }
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            model.IdKorisnik = user.Id;
            model.Projekti = _ctx.Projekt.Where(p => p.IdZbor == model.Novi.IdZbor).Include(p => p.ClanNaProjektu).Include(p => p.PrijavaZaProjekt);
            model.VrstePodjele = _ctx.VrstaPodjele.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Podjela }).ToList();
            model.IdZbor = model.Novi.IdZbor;
            return View(model);
            
        }

        [HttpPost]
        public async Task<IActionResult> PrijavaZaProjekt([FromBody] PrijavaModel prijava)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var idProjekt = Guid.Parse(prijava.Id);
            var prijavaZaProjekt = _ctx.PrijavaZaProjekt.Where(p => p.IdKorisnik == user.Id && p.IdProjekt == idProjekt).SingleOrDefault();
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
            _ctx.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> AdministracijaProjekta(Guid id)
        {
            AdministracijaProjektaViewModel model = new AdministracijaProjektaViewModel();
            var projekt = _ctx.Projekt.Where(z => z.Id == id)
                .Include(z => z.PozivZaProjekt).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.PrijavaZaProjekt).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(z => z.ClanNaProjektu).ThenInclude(c => c.IdKorisnikNavigation)
                .SingleOrDefault();
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!IsAdmin(projekt.IdZbor, user.Id)) 
                return RedirectToAction("Prava");
            model.Projekt = projekt;
            
            return View(model);
        }
        [HttpPost]
        public IActionResult PrihvatiPrijavuProjekt([FromBody] StringModel model)
        {
            PrijavaZaProjekt prijava = _ctx.PrijavaZaProjekt.Where(p => p.Id == Guid.Parse(model.Value)).Include(p => p.IdKorisnikNavigation).SingleOrDefault();
            ClanNaProjektu clan = new ClanNaProjektu
            {
                Id = Guid.NewGuid(),
                Uloga = "Nema",
                IdProjekt = prijava.IdProjekt,
                IdKorisnik = prijava.IdKorisnik
            };
            _ctx.ClanNaProjektu.Add(clan);
            _ctx.PrijavaZaProjekt.Remove(prijava);
            _ctx.SaveChanges();
            var m = new StringModel { Value = prijava.IdKorisnikNavigation.Ime + ' ' + prijava.IdKorisnikNavigation.Prezime };
            return Ok(m);
        }

        [HttpPost]
        public IActionResult OdbijPrijavuProjekt([FromBody] StringModel model)
        {
            PrijavaZaProjekt prijava = _ctx.PrijavaZaProjekt.Where(p => p.Id == Guid.Parse(model.Value)).SingleOrDefault();
            _ctx.PrijavaZaProjekt.Remove(prijava);
            _ctx.SaveChanges();
            var m = new StringModel { Value = "OKje" };

            return Ok(m);
        }

        [HttpPost]
        public IActionResult ObrisiPozivProjekt([FromBody] StringModel model)
        {
            PozivZaProjekt poziv = _ctx.PozivZaProjekt.Where(p => p.Id == Guid.Parse(model.Value)).SingleOrDefault();
            _ctx.PozivZaProjekt.Remove(poziv);
            _ctx.SaveChanges();
            var m = new StringModel { Value = "OKje" };

            return Ok(m);
        }
        public IActionResult OdbijPoziv(Guid id)
        {
            PozivZaZbor poziv = _ctx.PozivZaZbor.Where(p => p.Id == id).SingleOrDefault();
            _ctx.PozivZaZbor.Remove(poziv);
            _ctx.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult PrihvatiPoziv(Guid id)
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
            _ctx.ClanZbora.Add(clan);
            _ctx.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> PozivZaZbor([FromBody] PrijavaModel prijava)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var id = Guid.Parse(prijava.Id);
            var idZbor = Guid.Parse(prijava.Naziv);
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
            _ctx.PozivZaZbor.Add(pr);
            _ctx.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> PozivZaProjekt([FromBody] PrijavaModel prijava)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var id = Guid.Parse(prijava.Id);
            var idProjekt = Guid.Parse(prijava.Id);
            var pozivZaZbor = _ctx.PozivZaProjekt.Where(p => p.IdKorisnik == user.Id && p.IdProjekt == idProjekt).SingleOrDefault();
            if (pozivZaZbor != null)
                return Ok();
            var pr = new PozivZaProjekt
            {
                Id = Guid.NewGuid(),
                IdKorisnik = user.Id,
                IdProjekt = idProjekt,
                Poruka = prijava.Poruka,
                DatumPoziva = DateTime.Now
            };
            _ctx.PozivZaProjekt.Add(pr);
            _ctx.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IActionResult PretragaKorisnikaProjekt([FromBody] PretragaModel model)
        {
            Guid idProjekt = Guid.Parse(model.Id);
            var projekt = _ctx.Projekt.Where(p => p.Id == idProjekt).SingleOrDefault();
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
            var projekt = _ctx.Projekt.Where(p => p.Id == id).SingleOrDefault();
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (!IsAdmin(projekt.IdZbor, user.Id))
                return RedirectToAction("Prava");
            NoviDogadjajViewModel model = new NoviDogadjajViewModel
            {
                IdProjekt = id,
                VrsteDogadjaja = _ctx.VrstaDogadjaja.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Naziv }).ToList()

            };
            return View(model);
        }
        [HttpPost]
        public IActionResult NoviDogadjaj(Guid id, NoviDogadjajViewModel model)
        {
            if(ModelState.IsValid)
            {
                model.Novi.Id = Guid.NewGuid();
                _ctx.Dogadjaj.Add(model.Novi);
                _ctx.SaveChanges();
                return RedirectToAction("NoviDogadjaj", new { id = model.Novi.IdProjekt });
            }
            model.IdProjekt = model.Novi.IdProjekt;
            model.VrsteDogadjaja = _ctx.VrstaDogadjaja.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Naziv }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Kalendar(Guid id)
        {
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

            return View(new KalendarViewModel {Dogadjaji = dogadjaji, IdZbor = id });
        }
        [HttpPost]
        public async Task<IActionResult> UcitajClanove(Guid id, IFormFile file)
        {
            if (!file.FileName.EndsWith("xlsx"))
            {
                TempData["mess"] = "Uploadajte u xlsx formatu.";
            }



            if (file.Length > 0)
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
                                    if (glas.Equals("sopran") || glas.Equals("alt") || glas.Equals("tenor") || glas.Equals("bas")) ;
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
                                            Slika = "https://p7.hiclipart.com/preview/201/51/681/computer-icons-customer-service-user-others.jpg"
                                        };

                                        var result = await _userManager.CreateAsync(user, password);
                                        korisnik.Id = user.Id;
                                        korisnik.ClanZbora.Add(new ClanZbora { DatumPridruzivanja = DateTime.Now, IdKorisnik = user.Id, IdZbor = id, Id = Guid.NewGuid(), Glas = glas });

                                        _ctx.Add(korisnik);

                                        await _emailSender.SendEmailAsync(user.Email, "", "Vaša lozinka je: " + password);
                                    }
                                
                                
                                    else
                                    {
                                        var clan = _ctx.ClanZbora.Where(c => c.IdKorisnik == user.Id && c.IdZbor == id).FirstOrDefault();

                                        if (clan == null)
                                            _ctx.Add(new ClanZbora { DatumPridruzivanja = DateTime.Now, IdKorisnik = user.Id, IdZbor = id, Id = Guid.NewGuid(), Glas = glas });
                                    }
                                }
                                catch (Exception exc)
                                {
                                    continue;
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
            var projekt = _ctx.Projekt.Where(p => p.Id == id).Include(p => p.IdVrstePodjeleNavigation).Include(p => p.Dogadjaj).SingleOrDefault();
            var model = new ProjektViewModel { Admin = IsAdmin(projekt.IdZbor, user.Id), Projekt = projekt };
            model.AktivniDogadjaji = projekt.Dogadjaj.Where(d => d.DatumIvrijeme > DateTime.Now).OrderBy(d => d.DatumIvrijeme).AsEnumerable();
            model.ProsliDogadjaji = projekt.Dogadjaj.Where(d => d.DatumIvrijeme <= DateTime.Now).OrderByDescending(d => d.DatumIvrijeme).AsEnumerable();



            return View(model);

        }
        public async Task<IActionResult> NapustiZbor(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var clan = _ctx.ClanZbora.Where(c => c.IdKorisnik == user.Id && c.IdZbor == id).SingleOrDefault();
            _ctx.Remove(clan);
            _ctx.SaveChanges();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ObrisiDogadjaj(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dog = _ctx.Dogadjaj.Where(c => c.Id == id).SingleOrDefault();
            _ctx.Remove(dog);
            _ctx.SaveChanges();
            return RedirectToAction("Projekt" ,new { id = dog.IdProjekt });
        }
        [HttpGet]
        public async Task<IActionResult> UrediDogadjaj(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dog = _ctx.Dogadjaj.Where(c => c.Id == id).SingleOrDefault();
            var model = new NoviDogadjajViewModel
            {
                IdProjekt = dog.IdProjekt,
                Novi = dog,
                VrsteDogadjaja = _ctx.VrstaDogadjaja.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = v.Id.ToString(), Text = v.Naziv }).ToList()

            };
            return View(model);
        }

        [HttpGet("download")]
        public async Task<IActionResult> SkiniKalendar(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
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
    }
}
