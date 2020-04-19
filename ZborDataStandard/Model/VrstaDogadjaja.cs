using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class VrstaDogadjaja
    {
        public VrstaDogadjaja()
        {
            Dogadjaj = new HashSet<Dogadjaj>();
        }

        public Guid Id { get; set; }
        public string Naziv { get; set; }
        public string Opis { get; set; }

        public virtual ICollection<Dogadjaj> Dogadjaj { get; set; }
    }
}
