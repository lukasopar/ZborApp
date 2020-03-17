using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class PrijavaZaZbor
    {
        public Guid Id { get; set; }
        public Guid IdZbor { get; set; }
        public Guid IdKorisnik { get; set; }
        public DateTime DatumPrijave { get; set; }
        public string Poruka { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Zbor IdZborNavigation { get; set; }
    }
}
