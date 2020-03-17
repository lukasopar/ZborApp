using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Poruka
    {
        public Guid Id { get; set; }
        public string Poruka1 { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdRazgovor { get; set; }
        public DateTime DatumIvrijeme { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Razgovor IdRazgovorNavigation { get; set; }
    }
}
