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
using ZborDataStandard.Account;
using ZborDataStandard.Model;
using ZborApp.Models.KorisnikVIewModels;
using ZborApp.Models.AccountViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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
       

    }
}
