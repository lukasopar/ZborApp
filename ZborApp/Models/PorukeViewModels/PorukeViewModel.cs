using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.PorukeViewModels
{
    public class PorukeViewModel
    {
        public IList<Razgovor> Razgovori { get; set; }
        public Guid IdKorisnik { get; set; }
        public IList<Korisnik> Korisnici { get; set; }
    }
}
