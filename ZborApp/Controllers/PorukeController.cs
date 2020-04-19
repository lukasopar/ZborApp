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
using ZborApp.Models.PorukeViewModels;
using ZborApp.Models.ZborViewModels;
using ZborApp.Services;
using ZborDataStandard.Account;
using ZborDataStandard.Model;



namespace ZborApp.Controllers
{
    [Authorize]
    public class PorukeController : Controller
    {
        private readonly ILogger<ZborController> _logger;
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public PorukeController(ILogger<ZborController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
            _emailSender = new EmailSender();
        }
        public async Task<IActionResult> Index()
        {
            ViewData["id"] = TempData["id"]; 
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var korisnik = _ctx.Korisnik.Where(k => k.Id == user.Id).SingleOrDefault();
            var razg = _ctx.Razgovor.Where(r => r.KorisnikUrazgovoru.Select(kr => kr.IdKorisnik).Contains(korisnik.Id))
                .Include(r => r.KorisnikUrazgovoru).ThenInclude(kr => kr.IdKorisnikNavigation)
                .Include(r => r.Poruka).ThenInclude(p => p.IdKorisnikNavigation).OrderByDescending(r => r.DatumZadnjePoruke).ToList();
            PorukeViewModel model = new PorukeViewModel
            {
                Razgovori = razg,
                IdKorisnik = user.Id,
                Korisnici = _ctx.Korisnik.ToList()
            };
                return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Procitano([FromBody] StringModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idRazg;
            var flag = Guid.TryParse(model.Value, out idRazg);
            if (flag == false)
                return BadRequest();
            KorisnikUrazgovoru k = _ctx.KorisnikUrazgovoru.Where(k => k.IdKorisnik == user.Id && k.IdRazgovor == idRazg).SingleOrDefault();
            if (k == null)
                return NotFound();
            k.Procitano = true;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpPost]
        public async Task<IActionResult> PromjenaNaslova([FromBody] PretragaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            Guid idRazg;
            var flag = Guid.TryParse(model.Id, out idRazg);
            if (flag == false)
                return BadRequest();
            Razgovor r = _ctx.Razgovor.Find(idRazg);
            if (r == null)
                return NotFound();
            r.Naslov = model.Tekst;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpGet]
        public IActionResult Poruka(Guid id)
        {
            TempData["id"] = id;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DohvatiPoruke()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var poruke = _ctx.Razgovor.Where(r => r.KorisnikUrazgovoru.Select(k => k.IdKorisnik).Contains(user.Id)).OrderByDescending(r => r.DatumZadnjePoruke).Take(3)
                .Include(r => r.Poruka).ThenInclude(p => p.IdKorisnikNavigation)
                .Include(r => r.KorisnikUrazgovoru).ToList();
            int neprocitane = _ctx.Razgovor.Where(r => r.KorisnikUrazgovoru.Where(k => k.IdKorisnik == user.Id && k.Procitano == false).Count() > 0).Count();
            poruke.ForEach(r => r.Poruka = r.Poruka.OrderByDescending(p => p.DatumIvrijeme).ToList());
            var response = new
            {
                poruke = poruke.Select(r => new
                {
                    Id = r.Id,
                    Naziv = r.Naslov + " (" + r.Poruka.First().IdKorisnikNavigation.ImeIPrezime() + ")",
                    Datum = r.Poruka.First().DatumIvrijeme.ToString("dd.MM.yyyy. hh:mm"),
                    Slika = r.Poruka.First().IdKorisnikNavigation.IdSlika,
                    Poruka = r.Poruka.First().Poruka1,
                    Procitano = r.KorisnikUrazgovoru.Where(k => k.IdKorisnik == user.Id).SingleOrDefault().Procitano,
                }),
                Neprocitane = neprocitane

            };
            return Ok(response);
        }
        [HttpGet]
        public async Task<IActionResult> DohvatiNeprocitano()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            
            int neprocitane = _ctx.Razgovor.Where(r => r.KorisnikUrazgovoru.Where(k => k.IdKorisnik == user.Id && k.Procitano == false).Count() > 0).Count();
           
            return Ok(neprocitane);
        }
    }
}
