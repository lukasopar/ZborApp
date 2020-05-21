using System;
using System.Collections.Generic;
using System.Text;
using ZborDataStandard.Model;

namespace ZborMob.Model
{
    public class Login
    {
        public string Token { get; set; }
        public Korisnik Korisnik { get; set; }
    }
}
