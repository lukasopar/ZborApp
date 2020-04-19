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
using ZborDataStandard.ViewModels;
using ZborDataStandard.ViewModels.JSONModels;
using ZborDataStandard.ViewModels.ForumViewModels;
using ZborApp.Services;
using ZborDataStandard.Account;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.KorisnikVIewModels;

namespace ZborApp.Controllers
{
    [Authorize]
    public class KorisnikController : Controller
    {
        private readonly AppSettings appData = new AppSettings();

        private readonly ILogger<KorisnikController> _logger;
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public KorisnikController(ILogger<KorisnikController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
            _emailSender = new EmailSender();
        }
       
        [HttpGet]
        public async Task<IActionResult> Index(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var korisnik = _ctx.Korisnik.Where(k => k.Id == id).Include(k => k.ClanZbora).ThenInclude(c => c.IdZborNavigation).SingleOrDefault();
            if (korisnik == null)
                return RedirectToAction("Nema", "Greska");
            
            JavniProfilViewModel model = new JavniProfilViewModel
            {
                Korisnik = korisnik,
                Aktivni = user.Id
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Uredi([FromBody] PretragaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            _ctx.Korisnik.Find(user.Id).Opis = model.Tekst;
            _ctx.SaveChanges();
            return Ok();
        }
        public async Task<IActionResult> Galerija(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var korisnik = _ctx.Korisnik.Find(id);
            if (korisnik == null)
                return RedirectToAction("Nema", "Greska");
            var slike = _ctx.RepozitorijKorisnik.Where(z => z.IdKorisnik == id).ToList().Where(s => s.JeSlika()).ToList();
            if(user.Id != id)
            {
                slike = slike.Where(s => s.Privatno == false).ToList();
            }
            GalerijaViewModel model = new GalerijaViewModel
            {
                Slike = slike,
                Clan = user.Id == id,
                IdKorisnik = id
            };
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> DohvatiObavijesti()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var poruke = _ctx.OsobneObavijesti.Where(r => r.IdKorisnik == user.Id).OrderByDescending(r => r.Datum).Take(3).ToList();
            int neprocitane = _ctx.OsobneObavijesti.Where(r => r.IdKorisnik == user.Id && r.Procitano == false).Count();
            var response = new
            {
                poruke = poruke,
                Neprocitane = neprocitane

            };
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> DohvatiNeprocitano()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            int neprocitane = _ctx.OsobneObavijesti.Where(r => r.IdKorisnik == user.Id && r.Procitano == false).Count();

            return Ok(neprocitane);
        }
        [HttpPost]
        public async Task<IActionResult> ProcitajVrh()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var poruke = _ctx.OsobneObavijesti.Where(r => r.IdKorisnik == user.Id).OrderByDescending(r => r.Datum).Take(3).ToList();
            foreach (var o in poruke)
                o.Procitano = true;
            _ctx.SaveChanges();
            int neprocitane = _ctx.OsobneObavijesti.Where(r => r.IdKorisnik == user.Id && r.Procitano == false).Count();

            return Ok(neprocitane);
        }
        [HttpGet]
        public async Task<IActionResult> Obavijesti()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var model = new ObavijestiViewModel { Obavijesti = _ctx.OsobneObavijesti.Where(r => r.IdKorisnik == user.Id).OrderByDescending(r => r.Datum).AsNoTracking().ToList() };
            await _ctx.OsobneObavijesti.Where(o => o.IdKorisnik == user.Id && o.Procitano == false).ForEachAsync(o => o.Procitano = true);
            _ctx.SaveChanges();

            return View(model);
        }
    }
}
