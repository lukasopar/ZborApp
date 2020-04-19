using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class PrikazAnketeViewModel
    {
        public string DatumKraja { get; set; }
        public string AnketaJson { get; set; }
        public string Odgovor { get; set; }
        public string Rjesenje { get; set; }
        public Guid Id { get; set; }
        public Anketa Anketa { get; set; }
    }
}
