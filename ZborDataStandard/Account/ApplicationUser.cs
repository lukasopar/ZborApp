using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
namespace ZborDataStandard.Account
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string Token { get; set; }
    }
}
