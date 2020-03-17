using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class PozivZaProjekt
    {
        public Guid Id { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdProjekt { get; set; }
        public DateTime DatumPoziva { get; set; }
        public string Poruka { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Projekt IdProjektNavigation { get; set; }
    }
}
