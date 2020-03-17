using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class PitanjaViewModel
    {
        public Guid IdZbor { get; set; }
        public IEnumerable<Anketa> AktivnaPitanja { get; set; }
        public IEnumerable<Anketa> GotovaPitanja { get; set; }

        public bool Admin { get; set; }

    }
}
