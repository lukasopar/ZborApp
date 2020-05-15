using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class JavniProfilViewModel
    {
        public Zbor Zbor { get; set; }
        public bool Mod { get; set; }
        public bool Clan { get; set; }
        public bool Prijava { get; set; }
    }
}
