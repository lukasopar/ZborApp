using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class ModeratorZbora
    {
        public Guid Id { get; set; }
        public Guid IdZbor { get; set; }
        public Guid IdKorisnik { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Zbor IdZborNavigation { get; set; }
    }
}
