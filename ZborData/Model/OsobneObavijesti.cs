using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class OsobneObavijesti
    {
        public Guid Id { get; set; }
        public string Poveznica { get; set; }
        public string Tekst { get; set; }
        public bool Procitano { get; set; }
        public Guid IdKorisnik { get; set; }
        public DateTime Datum { get; set; } = DateTime.Now;
        public virtual Korisnik IdKorisnikNavigation { get; set; }
    }
}
