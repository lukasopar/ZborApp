using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class PitanjaViewModel
    {
        public Guid IdZbor { get; set; }
        public IEnumerable<Anketa> AktivnaPitanja { get; set; }
        public IEnumerable<Anketa> GotovaPitanja { get; set; }
        public Dictionary<Guid, List<int>> KorisnickiOdgovori = new Dictionary<Guid, List<int>>();

        public bool Admin { get; set; }
        public Guid IdKorisnik { get; set; }

        public string OdgovoriUListu(Guid id)
        {
            string rez = "[";
            foreach(var ko in KorisnickiOdgovori[id])
            {
                rez += ko + ",";
            }
            rez += "]";
            return rez;
        }
    }
}
