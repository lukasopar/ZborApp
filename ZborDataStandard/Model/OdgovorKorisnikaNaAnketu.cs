using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class OdgovorKorisnikaNaAnketu
    {
        public Guid Id { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdOdgovor { get; set; }
        public DateTime DatumOdgovora { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual OdgovorAnkete IdOdgovorNavigation { get; set; }
    }
}
