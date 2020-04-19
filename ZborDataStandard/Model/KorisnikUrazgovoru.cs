using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class KorisnikUrazgovoru
    {
        public Guid Id { get; set; }
        public Guid IdRazgovor { get; set; }
        public Guid IdKorisnik { get; set; }
        public bool Procitano { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Razgovor IdRazgovorNavigation { get; set; }
    }
}
