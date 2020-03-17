using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class LajkKomentara
    {
        public Guid Id { get; set; }
        public Guid IdKomentar { get; set; }
        public Guid IdKorisnik { get; set; }

        public virtual KomentarObavijesti IdKomentarNavigation { get; set; }
        public virtual Korisnik IdKorisnikNavigation { get; set; }
    }
}
