using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class PretplataNaZbor
    {
        public Guid Id { get; set; }
        public Guid IdZbor { get; set; }
        public Guid IdKorisnik { get; set; }
        public bool Pitanja { get; set; }
        public bool Obavijesti { get; set; }
        public bool Repozitorij { get; set; }
        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Zbor IdZborNavigation { get; set; }

    }
}
