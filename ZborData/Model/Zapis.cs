using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Zapis
    {
        public Zapis()
        {
            InverseIdZapisNavigation = new HashSet<Zapis>();
        }

        public Guid Id { get; set; }
        public Guid IdZapis { get; set; }
        public Guid IdKorisnik { get; set; }
        public string Tekst { get; set; }
        public DateTime DatumIvrijeme { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Zapis IdZapisNavigation { get; set; }
        public virtual ICollection<Zapis> InverseIdZapisNavigation { get; set; }
    }
}
