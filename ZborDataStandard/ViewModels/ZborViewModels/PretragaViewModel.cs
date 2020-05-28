using System;
using System.Collections.Generic;
using System.Text;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class PretragaViewModel
    {
        public ICollection<Zbor> Zborovi { get; set; }
        public ICollection<Korisnik> Korisnici { get; set; }
    }
}
