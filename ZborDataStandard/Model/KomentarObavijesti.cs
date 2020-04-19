using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class KomentarObavijesti
    {
        public KomentarObavijesti()
        {
            LajkKomentara = new HashSet<LajkKomentara>();
        }

        public Guid Id { get; set; }
        public Guid IdObavijest { get; set; }
        public Guid IdKorisnik { get; set; }
        public string Tekst { get; set; }
        public DateTime DatumObjave { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Obavijest IdObavijestNavigation { get; set; }
        public virtual ICollection<LajkKomentara> LajkKomentara { get; set; }
    }
}
