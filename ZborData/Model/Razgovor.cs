using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Razgovor
    {
        public Razgovor()
        {
            KorisnikUrazgovoru = new HashSet<KorisnikUrazgovoru>();
            Poruka = new HashSet<Poruka>();
        }

        public Guid Id { get; set; }
        public string Naslov { get; set; }
        public DateTime DatumZadnjePoruke { get; set; }

        public virtual ICollection<KorisnikUrazgovoru> KorisnikUrazgovoru { get; set; }
        public virtual ICollection<Poruka> Poruka { get; set; }
    }
}
