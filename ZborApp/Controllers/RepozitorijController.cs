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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ZborApp.Models;
using ZborApp.Models.JSONModels;
using ZborApp.Models.PorukeViewModels;
using ZborApp.Models.RepozitorijViewModels;
using ZborApp.Models.ZborViewModels;
using ZborApp.Services;
using ZborData.Account;
using ZborData.Model;



namespace ZborApp.Controllers
{
    [Authorize]
    public class RepozitorijController : Controller
    {
        private const string LOCATION = "E:/UploadZbor/";
        private const string LOCATION_CHOIR = "E:/UploadZbor/Zbor/";

        private readonly ILogger<RepozitorijController> _logger;
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        public RepozitorijController(ILogger<RepozitorijController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
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
            var datoteke = _ctx.RepozitorijZbor.Where(r => r.IdZbor == id).OrderByDescending(r => r.DatumPostavljanja).ToList();
            if (!_ctx.ClanZbora.Select(c => c.IdKorisnik).Contains(id))
                datoteke = datoteke.Where(d => d.Privatno == false).ToList();

            RepozitorijZborViewModel model = new RepozitorijZborViewModel
            {
                Datoteke = datoteke,
                IdKorisnik = user.Id,
                IdTrazeni = id,
                IdZbor = id
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Objavi([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            //provjera usera bla bla
            var d = _ctx.RepozitorijKorisnik.Where(d => d.Id == Guid.Parse(model.Value)).SingleOrDefault();
            d.Privatno = false;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpPost]
        public async Task<IActionResult> ObjaviZbor([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            //provjera usera bla bla
            var d = _ctx.RepozitorijZbor.Where(d => d.Id == Guid.Parse(model.Value)).SingleOrDefault();
            d.Privatno = false;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpPost]
        public async Task<IActionResult> Privatiziraj([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            //provjera usera bla bla
            var d = _ctx.RepozitorijKorisnik.Where(d => d.Id == Guid.Parse(model.Value)).SingleOrDefault();
            d.Privatno = true;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpPost]
        public async Task<IActionResult> PrivatizirajZbor([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            //provjera usera bla bla
            var d = _ctx.RepozitorijZbor.Where(d => d.Id == Guid.Parse(model.Value)).SingleOrDefault();
            d.Privatno = true;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpPost]
        public IActionResult PromjenaNaziva([FromBody] PretragaModel model)
        {
            var d = _ctx.RepozitorijKorisnik.Where(d => d.Id == Guid.Parse(model.Id)).SingleOrDefault();
            d.Naziv = model.Tekst + "." + d.GetEkstenzija();
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpPost]
        public IActionResult PromjenaNazivaZbor([FromBody] PretragaModel model)
        {
            var d = _ctx.RepozitorijZbor.Where(d => d.Id == Guid.Parse(model.Id)).SingleOrDefault();
            d.Naziv = model.Tekst + "." + d.GetEkstenzija();
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

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

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
                    var filePath = model.IdZbor+ "/" + formFile.FileName;
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
                _ctx.SaveChanges();
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return RedirectToAction("Zbor", new { id = user.Id });
        }
        public async Task<IActionResult> Delete(RepozitorijViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dat = _ctx.RepozitorijKorisnik.Where(d => d.Id == model.IdTrazeni).SingleOrDefault();
            try
            {

                System.IO.File.Delete(LOCATION + "/" + dat.Url);
                _ctx.Remove(dat);
                _ctx.SaveChanges();
            }catch (Exception exc)
            {
                int g = 0; ;
            };


            return RedirectToAction("Korisnik", new { id = user.Id });
        }
        public async Task<IActionResult> DeleteZbor(RepozitorijZborViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dat = _ctx.RepozitorijZbor.Where(d => d.Id == model.IdTrazeni).SingleOrDefault();
            try
            {

                System.IO.File.Delete(LOCATION_CHOIR + "/" + dat.Url);
                _ctx.Remove(dat);
                _ctx.SaveChanges();
            }
            catch (Exception exc)
            {
                int g = 0; ;
            };


            return RedirectToAction("Zbor", new { id = model.IdZbor });
        }
        public async Task<IActionResult> Get(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dat = _ctx.RepozitorijKorisnik.Where(d => d.Id == id).SingleOrDefault();
            
            return File(System.IO.File.ReadAllBytes(LOCATION + "/" + dat.Url), "application/force-download", dat.Naziv);
        }
        public async Task<IActionResult> GetZbor(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var dat = _ctx.RepozitorijZbor.Where(d => d.Id == id).SingleOrDefault();

            return File(System.IO.File.ReadAllBytes(LOCATION_CHOIR + "/" + dat.Url), "application/force-download", dat.Naziv);
        }
    }
}

   
