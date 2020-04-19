using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Zbor> MojiZborovi { get; set; }
        public IEnumerable<Zbor> OstaliZborovi { get; set; }
        public IEnumerable<Zbor> PoslanePrijaveZborovi { get; set; }
        public IEnumerable<PozivZaZbor> MojiPozivi { get; set; }

        public Guid KorisnikId { get; set; }

    }
}
