using System;
using System.Collections.Generic;
using System.Text;
using ZborDataStandard.Model;

namespace ZborMob.Model
{
    public class Statistika
    {
        public List<Dogadjaj> Evidentirani { get; set; }
        public List<Dogadjaj> Neevidentirani { get; set; }

        public double Postotak { get; set; }
    }
}
