using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class IndexViewModel
    {
        public List<Zbor> MojiZborovi { get; set; }
        public List<Zbor> OstaliZborovi { get; set; }
        public List<Zbor> PoslanePrijaveZborovi { get; set; }
        public List<PozivZaZbor> MojiPozivi { get; set; }

        public Guid KorisnikId { get; set; }

    }
}
