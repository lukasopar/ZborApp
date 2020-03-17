using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class LajkObavijesti
    {
        public Guid Id { get; set; }
        public Guid IdObavijest { get; set; }
        public Guid IdKorisnik { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Obavijest IdObavijestNavigation { get; set; }
    }
}
