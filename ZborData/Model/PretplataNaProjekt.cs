using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class PretplataNaProjekt
    {
        public Guid Id { get; set; }
        public Guid IdProjekt { get; set; }
        public Guid IdKorisnik { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Projekt IdProjektNavigation { get; set; }
    }
}
