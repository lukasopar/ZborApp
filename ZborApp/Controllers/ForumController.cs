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
using ZborApp.Models.ForumViewModels;
using ZborApp.Services;
using ZborData.Account;
using ZborData.Model;



namespace ZborApp.Controllers
{
    [Authorize]
    public class ForumController : Controller
    {
        private readonly AppSettings appData = new AppSettings();

        private readonly ILogger<ZborController> _logger;
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public ForumController(ILogger<ZborController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
            _emailSender = new EmailSender();
        }
        public async Task<IActionResult> Index()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var model = new IndexViewModel
            {
                KategorijaForuma = _ctx.KategorijaForuma.Include(k => k.Forum).ThenInclude(f => f.Tema).OrderBy(k => k.Redoslijed).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult Index(IndexViewModel model)
        {
            if(ModelState.IsValid)
            {
                model.Novi.Id = Guid.NewGuid();
                _ctx.Forum.Add(model.Novi);
                _ctx.SaveChanges();
                return RedirectToAction("Index");
            }
            model.KategorijaForuma = _ctx.KategorijaForuma.Include(k => k.Forum).ThenInclude(f => f.Tema).OrderBy(k => k.Redoslijed).ToList();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Tema(TemeViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (ModelState.IsValid)
            {
                model.Nova.Id = Guid.NewGuid();
                Zapis zap = new Zapis
                {
                    Id = Guid.NewGuid(),
                    IdKorisnik = user.Id,
                    DatumIvrijeme = DateTime.Now,
                    IdTema = model.Nova.Id,
                    Tekst = model.Tekst
                };
                model.Nova.DatumPocetka = zap.DatumIvrijeme;
                model.Nova.ZadnjiZapis = zap.DatumIvrijeme;
                model.Nova.Zapis.Add(zap);
                model.Nova.IdKorisnik = user.Id;
                model.Nova.IdForum = model.IdForum;
                _ctx.Tema.Add(model.Nova);
                _ctx.SaveChanges();
                return RedirectToAction("Tema", new { id = model.IdForum});
            }
            //model.KategorijaForuma = _ctx.KategorijaForuma.Include(k => k.Forum).ThenInclude(f => f.Tema).OrderBy(k => k.Redoslijed).ToList();
            return View(model);
        }
        [HttpGet]
        public IActionResult Tema(Guid id, [FromQuery] int page = 1)
        {
            int pagesize = appData.PageSize;

            var teme = _ctx.Tema.Where(t => t.IdForum == id);
            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = pagesize,
                TotalItems = teme.Count()
            };

            if (page > pagingInfo.TotalPages && pagingInfo.TotalItems != 0)
            {
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalPages });
            }

            System.Linq.Expressions.Expression<Func<Tema, object>> orderSelector = t => t.ZadnjiZapis;
            
            if (orderSelector != null)
            {
                teme = 
                       
                     
                       _ctx.Tema.Where(t => t.IdForum == id).Include(t => t.IdForumNavigation).Include(t => t.Zapis).ThenInclude(z => z.IdKorisnikNavigation).OrderByDescending(orderSelector);
            }
            var trazeneTeme = teme
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToList();
            var model = new TemeViewModel()
            {
                Teme = trazeneTeme,
                PagingInfo = pagingInfo,
                IdForum = id
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Zapis(Guid id, [FromQuery] int page = 1)
        {
            int pagesize = appData.PageSize;

            var teme = _ctx.Zapis.Where(t => t.IdTema == id);
            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = pagesize,
                TotalItems = teme.Count()
            };

            if (page > pagingInfo.TotalPages && pagingInfo.TotalItems != 0)
            {
                return RedirectToAction(nameof(Index), new { page = pagingInfo.TotalPages });
            }

            System.Linq.Expressions.Expression<Func<Zapis, object>> orderSelector = t => t.DatumIvrijeme;

            if (orderSelector != null)
            {
                teme =


                       _ctx.Zapis.Where(t => t.IdTema == id).Include(t => t.IdTemaNavigation).Include(t => t.IdKorisnikNavigation).OrderBy(orderSelector);
            }
            var trazeniZapis = teme
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToList();
            var model = new ZapisVIewModel()
            {
                Zapisi = trazeniZapis,
                PagingInfo = pagingInfo,
                IdTema = id
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Zapis(ZapisVIewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (ModelState.IsValid)
            {
                model.Novi.Id = Guid.NewGuid();
                model.Novi.IdKorisnik = user.Id;
                model.Novi.DatumIvrijeme = DateTime.Now;
               
                _ctx.Zapis.Add(model.Novi);
                _ctx.SaveChanges();
                return RedirectToAction("Zapis", new { id = model.Novi.IdTema });
            }
            //model.KategorijaForuma = _ctx.KategorijaForuma.Include(k => k.Forum).ThenInclude(f => f.Tema).OrderBy(k => k.Redoslijed).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Uredi([FromBody] PretragaModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            _ctx.Zapis.Find(Guid.Parse(model.Id)).Tekst = model.Tekst;
            _ctx.SaveChanges();
            return Ok();
        }
    }
}
