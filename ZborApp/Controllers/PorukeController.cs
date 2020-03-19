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
using ZborData.Account;
using ZborData.Model;



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
            KorisnikUrazgovoru k = _ctx.KorisnikUrazgovoru.Where(k => k.IdKorisnik == user.Id && k.IdRazgovor == Guid.Parse(model.Value)).SingleOrDefault();
            k.Procitano = true;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
        [HttpPost]
        public IActionResult PromjenaNaslova([FromBody] PretragaModel model)
        {
            Razgovor r = _ctx.Razgovor.Where(r => r.Id == Guid.Parse(model.Id)).SingleOrDefault();
            r.Naslov = model.Tekst;
            _ctx.SaveChanges();
            var m = new StringModel { Value = "Ok" };
            return Ok(m);
        }
    }
}
