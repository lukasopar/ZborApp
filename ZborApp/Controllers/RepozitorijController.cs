using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using ZborDataStandard.ViewModels;
using ZborDataStandard.ViewModels.JSONModels;
using ZborDataStandard.ViewModels.PorukeViewModels;
using ZborDataStandard.ViewModels.RepozitorijViewModels;
using ZborDataStandard.ViewModels.ZborViewModels;
using ZborApp.Services;
using ZborDataStandard.Account;
using ZborDataStandard.Model;



namespace ZborApp.Controllers
{
    [Authorize]
    public class RepozitorijController : Controller
    {
        private const string LOCATION = "C:/UploadZbor/";
        private const string LOCATION_CHOIR = "C:/UploadZbor/Zbor/";

        private readonly ILogger<RepozitorijController> _logger;
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public RepozitorijController(ILogger<RepozitorijController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
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
        public async Task<IActionResult> Korisnik(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
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
            return View(model);
        }
        public async Task<IActionResult> Zbor(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var korisnik = _ctx.Korisnik.Where(k => k.Id == user.Id).SingleOrDefault();
            var datoteke = _ctx.RepozitorijZbor.Where(r => r.IdZbor == id).Include(k => k.IdKorisnikNavigation).OrderByDescending(r => r.DatumPostavljanja).ToList();
            if (!_ctx.ClanZbora.Where(c => c.IdZbor == id).Select(c => c.IdKorisnik).Contains(user.Id))
                datoteke = datoteke.Where(d => d.Privatno == false).ToList();

            var zbor = _ctx.Zbor.Where(z => z.Id == id).Include(z => z.Voditelj).Include(z => z.ModeratorZbora).SingleOrDefault();
            var admin = zbor.Voditelj.OrderByDescending(z => z.DatumPostanka).First();
            var mod = zbor.ModeratorZbora.Where(m => m.IdKorisnik == user.Id).SingleOrDefault();
            bool flagAdmin  =  admin.IdKorisnik == user.Id || mod != null ? true : false;
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
            ViewData["zborId"] = id;
            ViewData["zborIme"] = zbor.Naziv;
            ViewData["idSlika"] = zbor.IdSlika;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Objavi([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
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
        public async Task<IActionResult> ObjaviZbor([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
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
        public async Task<IActionResult> Privatiziraj([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
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
        [HttpPost]
        public async Task<IActionResult> PrivatizirajZbor([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
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
        [HttpPost]
        public async Task<IActionResult> PromjenaNaziva([FromBody] PretragaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idRep;
            var flag = Guid.TryParse(model.Id, out idRep);
            if (flag == false)
                return BadRequest();
            var d = _ctx.RepozitorijKorisnik.Find(idRep);
            if (d == null)
                return NotFound();
            if (d.IdKorisnik != user.Id)
                return Forbid();
            d.Naziv = model.Tekst.Trim() + "." + d.GetEkstenzija();
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpPost]
        public async Task<IActionResult> PromjenaNazivaZbor([FromBody] PretragaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idRep;
            var flag = Guid.TryParse(model.Id, out idRep);
            if (flag == false)
                return BadRequest();
            var d = _ctx.RepozitorijZbor.Find(idRep);
            if (d == null)
                return NotFound();
            if (!IsAdmin(d.IdZbor, user.Id) && d.IdKorisnik != user.Id)
                return Forbid();
            d.Naziv = model.Tekst.Trim() + "." + d.GetEkstenzija();
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        public async Task<IActionResult> Upload(RepozitorijViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var files = model.FormFiles;
            long size = files.Sum(f => f.Length);

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    DirectoryInfo di = Directory.CreateDirectory(LOCATION + "/" + user.Id);
                    var filePath = user.Id + "/" + formFile.FileName;
                    RepozitorijKorisnik dat = new RepozitorijKorisnik
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

            return RedirectToAction("Korisnik", new { id = user.Id });
        }
        public async Task<IActionResult> UploadZbor(RepozitorijZborViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var files = model.FormFiles;
            long size = files.Sum(f => f.Length);

            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    DirectoryInfo di = Directory.CreateDirectory(LOCATION_CHOIR + model.IdZbor);
                    var filePath = "Zbor/"+ model.IdZbor+ "/" + formFile.FileName;
                    RepozitorijZbor dat = new RepozitorijZbor
                    {
                        Id = Guid.NewGuid(),
                        DatumPostavljanja = DateTime.Now,
                        IdKorisnik = user.Id,
                        IdZbor = model.IdZbor,
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
                var pretplatnici = _ctx.PretplataNaZbor.Where(p => p.IdZbor == model.IdZbor).Select(p => p.IdKorisnik).ToHashSet();
                foreach (var pret in pretplatnici)
                {
                    OsobneObavijesti ob = new OsobneObavijesti
                    {
                        Id = Guid.NewGuid(),
                        IdKorisnik = pret,
                        Tekst = String.Format("Nove datoteke u zboru <b>{0}</b>.", _ctx.Zbor.Find(model.IdZbor).Naziv),
                        Procitano = false,
                        Poveznica = "/Repozitorij/Zbor/" + model.IdZbor
                    };
                    _ctx.Add(ob);
                    await _hubContext.Clients.User(pret.ToString()).SendAsync("NovaObavijest", new { id = ob.Id, poveznica = ob.Poveznica, procitano = ob.Procitano, datum = ob.Datum, tekst = ob.Tekst });

                }
                _ctx.SaveChanges();
            }

            return RedirectToAction("Zbor", new { id = model.IdZbor });
        }
        public async Task<IActionResult> Delete(RepozitorijViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
 
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
            }catch (Exception)
            {
                return RedirectToAction("Error", "Zbor");
            };


            return RedirectToAction("Korisnik", new { id = user.Id });
        }
        public async Task<IActionResult> DeleteZbor(RepozitorijZborViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dat = _ctx.RepozitorijZbor.Find(model.IdTrazeni);
            if (dat == null)
                return RedirectToAction("Nema", "Greska");
            if (!IsAdmin(dat.IdZbor, user.Id) && dat.IdKorisnik != user.Id)
                return RedirectToAction("Prava", "Zbor");
            try
            {

                System.IO.File.Delete(LOCATION + "/" + dat.Url);
                _ctx.Remove(dat);
                _ctx.SaveChanges();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Zbor");
            };


            return RedirectToAction("Zbor", new { id = dat.IdZbor });
        }
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dat = _ctx.RepozitorijKorisnik.Where(d => d.Id == id).SingleOrDefault();
            if (dat == null)
                return View("Nema", "Greska");
            return File(System.IO.File.ReadAllBytes(LOCATION + "/" + dat.Url), "application/force-download", dat.Naziv);
            
        }
        [AllowAnonymous]
        public async Task<IActionResult> GetZbor(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dat = _ctx.RepozitorijZbor.Where(d => d.Id == id).SingleOrDefault();
            if (dat == null)
                return View("Nema", "Greska");
            return File(System.IO.File.ReadAllBytes(LOCATION + "/" + dat.Url), "application/force-download", dat.Naziv);
        }
        public async Task<IActionResult> PromjenaProfilne(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dat = _ctx.RepozitorijKorisnik.Find(id);
            if (dat == null)
                return View("Nema", "Greska");
            if (user.Id != dat.IdKorisnik)
                return View("Prava", "Zbor");
            dat.Privatno = false;
            _ctx.Korisnik.Find(user.Id).IdSlika = id;
            _ctx.SaveChanges();
            return RedirectToAction("Galerija","Korisnik" ,new { id = user.Id });
        }
        public async Task<IActionResult> PromjenaProfilneZbor(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dat = _ctx.RepozitorijZbor.Find(id);
            if (dat == null)
                return View("Nema", "Greska");
            if (!CheckRights(dat.IdZbor, user.Id))
                return View("Prava", "Zbor");
            dat.Privatno = false;
            _ctx.Zbor.Find(dat.IdZbor).IdSlika = id;
            _ctx.SaveChanges();
            return RedirectToAction("Galerija", "Zbor", new { id = dat.IdZbor });
        }
       
    }
}

   
