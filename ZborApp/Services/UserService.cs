using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ZborDataStandard.Account;
using ZborDataStandard.Model;

namespace ZborApp.Services
{
    public interface IUserService
    {
        Task<ApplicationUser> Authenticate(string username, string password);
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private ZborDatabaseContext _ctx;
        private readonly AppSettings _appSettings;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        public UserService(IOptions<AppSettings> appSettings, ZborDatabaseContext ctx, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _appSettings = appSettings.Value;
            _ctx = ctx;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ApplicationUser> Authenticate(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            var valid = await _userManager.CheckPasswordAsync(user, password);
            // return null if user not found
            if (!valid)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            await _userManager.UpdateAsync(user);
            _ctx.SaveChanges();
            return user;
        }

   
    }
   
}
