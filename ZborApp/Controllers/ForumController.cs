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



namespace ZborApp.Controllers
{
    [Authorize]
    public class ForumController : Controller
    {
        private readonly AppSettings appData = new AppSettings();

        private readonly ILogger<ForumController> _logger;
        private readonly ZborDatabaseContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public ForumController(ILogger<ForumController> logger, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _ctx = ctx;
            _userManager = userManager;
            _emailSender = new EmailSender();
        }
        private bool isAdmin(Guid id)
        {
            return _ctx.AdministratorForuma.Find(id) == null ? false : true;
        }
        private bool isMod(Guid id)
        {
            return _ctx.AdministratorForuma.Find(id) == null ? false : true || _ctx.ModForum.Find(id) == null?false:true;
        }
        public async Task<IActionResult> Index()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            bool mod = isMod(user.Id);
            var model = new IndexViewModel
            {
                KategorijaForuma = _ctx.KategorijaForuma.Include(k => k.Forum).ThenInclude(f => f.Tema).OrderBy(k => k.Redoslijed).ToList(),
                Mod = mod,
                Admin = isAdmin(user.Id)
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Index(IndexViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            bool mod = isMod(user.Id);
            if (!mod)
            {
                return RedirectToAction("Prava", "Zbor");
            }
            if (model.Novi.Naziv.Trim().Equals(""))
                ModelState.AddModelError("Naziv", "Naziv je obavezan");
            if (model.Novi.Opis.Trim().Equals(""))
                ModelState.AddModelError("Opis", "Opis je obavezan");
            if(_ctx.Forum.Where(f => f.Naziv.Equals(model.Novi.Naziv)).SingleOrDefault() != null)
            {
                ModelState.AddModelError("Opis", "Podforum ovog naziva već postoji.");

            }
            if (ModelState.IsValid)
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
            if (model.Nova.Naslov.Trim().Equals(""))
                ModelState.AddModelError("Naslov", "Naslov je obavezan");
            if (model.Tekst.Trim().Equals(""))
                ModelState.AddModelError("Tekst", "Zapis je obavezan");
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
        public async Task<IActionResult> Tema(Guid id, [FromQuery] int page = 1)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            bool mod = isMod(user.Id);
            int pagesize = appData.PageSize;
            var forum = _ctx.Forum.Find(id);
            if (forum == null)
            {
                return RedirectToAction("Nema", "Greska");
            }
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
                IdForum = id,
                Mod = mod,
                IdKorisnik = user.Id,
                Naslov = forum.Naziv
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Zapis(Guid id, [FromQuery] int page = 1)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            bool mod = isMod(user.Id);
            int pagesize = appData.PageSize;
            var tema = _ctx.Tema.Find(id);
            if (tema == null)
            {
                return RedirectToAction("Nema", "Greska");
            }
            var teme = _ctx.Zapis.Where(t => t.IdTema == id);
            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = pagesize,
                TotalItems = teme.Count()
            };
            if(page == 0)
            {
                return RedirectToAction(nameof(Zapis), new {id=id, page = pagingInfo.TotalPages });
            }
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
                IdTema = id,
                IdKorisnik = user.Id,
                Mod = mod,
                Naslov = tema.Naslov
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Zapis(ZapisVIewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            if (model.Novi.Tekst.Trim().Equals(""))
                ModelState.AddModelError("Tekst", "Zapis je obavezan");
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
            Guid idZapis;
            var flag = Guid.TryParse(model.Id, out idZapis);
            if (flag == false)
                return BadRequest();
            var zapis = _ctx.Zapis.Find(idZapis);
            if (zapis == null)
                return NotFound();
            if (!isMod(user.Id) && zapis.IdKorisnik != user.Id)
                return Forbid();
            zapis.Tekst = model.Tekst;
            _ctx.SaveChanges();
            return Ok();
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiZapis(ZapisVIewModel model, int page = 1)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            var zapis = _ctx.Zapis.Find(model.IdBrisanje);
            if(zapis == null)
                return RedirectToAction("Nema", "Greska");
            if (!isMod(user.Id) && zapis.IdKorisnik != user.Id)
                return RedirectToAction("Prava", "Zbor");
            _ctx.Zapis.Remove(zapis);
            _ctx.SaveChanges();
            return RedirectToAction("Zapis", new { id = zapis.IdTema, page = page });
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiTema(TemeViewModel model, int page = 1)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);
            var zapis = _ctx.Tema.Find(model.IdBrisanje);
            if (zapis == null)
                return RedirectToAction("Nema", "Greska");
            if (!isMod(user.Id) && zapis.IdKorisnik != user.Id)
                return RedirectToAction("Prava", "Zbor");
            _ctx.Tema.Remove(zapis);
            _ctx.SaveChanges();
            return RedirectToAction("Tema", new { id = zapis.IdForum, page = page });
        }
        [HttpGet]
        public async Task<IActionResult> Administracija()
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (!isAdmin(user.Id))
                return RedirectToAction("Prava", "Zbor");
            AdministracijaViewModel model = new AdministracijaViewModel
            {

                Administratori = _ctx.AdministratorForuma.Include(a => a.IdKorisnikNavigation).ToList(),
                Moderatori = _ctx.ModForum.Include(a => a.IdKorisnikNavigation).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public IActionResult Pretraga([FromBody] PretragaModel model)
        {
            var korisnici = _ctx.Korisnik.Where(k => (k.Ime.Trim().ToLower() + ' ' + k.Prezime.Trim().ToLower()).Contains(model.Tekst))
                .Select(k => new { Id = k.Id, ImeIPrezime = k.Ime.Trim() + ' ' + k.Prezime.Trim() })
                .ToList();

            return Ok(korisnici);
        }
        [HttpGet]
        public async Task<IActionResult> PostaviAdministratora(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (!isAdmin(user.Id))
                return RedirectToAction("Prava", "Zbor");
            var admin = _ctx.AdministratorForuma.Find(id);
            if(admin != null)
            {
                ViewData["alert"] = "Korisnik već je administrator";
                
            }
            else
            {
                _ctx.AdministratorForuma.Add(new AdministratorForuma { Id = id });
                ViewData["alert"] = "Dodan administrator";
                _ctx.SaveChanges();

            }

            AdministracijaViewModel model = new AdministracijaViewModel
            {

                Administratori = _ctx.AdministratorForuma.Include(a => a.IdKorisnikNavigation).ToList(),
                Moderatori = _ctx.ModForum.Include(a => a.IdKorisnikNavigation).ToList()
            };
            return View("Administracija", model);
        }
        [HttpGet]
        public async Task<IActionResult> PostaviModeratora(Guid id)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (!isAdmin(user.Id))
                return RedirectToAction("Prava", "Zbor");
            var mod = _ctx.ModForum.Find(id);
            if (mod != null)
            {
                ViewData["alert"] = "Korisnik već je moderator";

            }
            else
            {
                _ctx.ModForum.Add(new ModForum { Id = id });
                ViewData["alert"] = "Dodan moderator";
                _ctx.SaveChanges();
            }
            
            AdministracijaViewModel model = new AdministracijaViewModel
            {

                Administratori = _ctx.AdministratorForuma.Include(a => a.IdKorisnikNavigation).ToList(),
                Moderatori = _ctx.ModForum.Include(a => a.IdKorisnikNavigation).ToList()
            };
            return View("Administracija", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ObrisiAdministratora(AdministracijaViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (!isAdmin(user.Id))
                return RedirectToAction("Prava", "Zbor");
            var zap = _ctx.AdministratorForuma.Find(model.IdCilj);
            if(zap == null)
                return RedirectToAction("Administracija");

            _ctx.AdministratorForuma.Remove(zap);
            _ctx.SaveChanges();
            return RedirectToAction("Administracija");
        }
        [HttpPost]
        public async Task<IActionResult> ObrisiModeratora(AdministracijaViewModel model)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (!isAdmin(user.Id))
                return RedirectToAction("Prava", "Zbor");
            var zap = _ctx.ModForum.Find(model.IdCilj);
            if (zap == null)
                return RedirectToAction("Administracija");

            _ctx.ModForum.Remove(zap);
            _ctx.SaveChanges();
            return RedirectToAction("Administracija");
        }
    }
}
