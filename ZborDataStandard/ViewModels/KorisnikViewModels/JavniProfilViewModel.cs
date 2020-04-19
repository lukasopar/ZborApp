using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.KorisnikVIewModels
{
    public class JavniProfilViewModel
    {
 
        public Korisnik Korisnik { get; set; }
        public Guid Aktivni { get; set; }
    }
}
