using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Tema
    {
        public Guid Id { get; set; }
        public string Naslov { get; set; }
        public DateTime DatumPocetka { get; set; }
        public Guid IdKorisnik { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
    }
}
