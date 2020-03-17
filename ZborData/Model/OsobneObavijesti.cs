using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class OsobneObavijesti
    {
        public Guid Id { get; set; }
        public string Naslov { get; set; }
        public string Tekst { get; set; }
        public bool Procitano { get; set; }
        public Guid IdKorisnik { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
    }
}
