using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Anketa
    {
        public Anketa()
        {
            OdgovorAnkete = new HashSet<OdgovorAnkete>();
        }

        public Guid Id { get; set; }
        public Guid IdZbor { get; set; }
        public Guid IdKorisnik { get; set; }
        public string Pitanje { get; set; }
        public bool VisestrukiOdgovor { get; set; }
        public DateTime DatumPostavljanja { get; set; }
        public DateTime DatumKraja { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Zbor IdZborNavigation { get; set; }
        public virtual ICollection<OdgovorAnkete> OdgovorAnkete { get; set; }
    }
}
