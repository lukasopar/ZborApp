using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.PorukeViewModels
{
    public class PorukeViewModel
    {
        public IList<Razgovor> Razgovori { get; set; }
        public Guid IdKorisnik { get; set; }
        public IList<Korisnik> Korisnici { get; set; }
    }
}
