using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Zbor
    {
        public Zbor()
        {
            Anketa = new HashSet<Anketa>();
            ClanZbora = new HashSet<ClanZbora>();
            ModeratorZbora = new HashSet<ModeratorZbora>();
            Obavijest = new HashSet<Obavijest>();
            PozivZaZbor = new HashSet<PozivZaZbor>();
            PrijavaZaZbor = new HashSet<PrijavaZaZbor>();
            Projekt = new HashSet<Projekt>();
            Voditelj = new HashSet<Voditelj>();
        }

        public Guid Id { get; set; }
        public string Naziv { get; set; }
        public string Adresa { get; set; }
        public string Opis { get; set; }
        public DateTime DatumOsnutka { get; set; }

        public virtual ICollection<Anketa> Anketa { get; set; }
        public virtual ICollection<ClanZbora> ClanZbora { get; set; }
        public virtual ICollection<ModeratorZbora> ModeratorZbora { get; set; }
        public virtual ICollection<Obavijest> Obavijest { get; set; }
        public virtual ICollection<PozivZaZbor> PozivZaZbor { get; set; }
        public virtual ICollection<PrijavaZaZbor> PrijavaZaZbor { get; set; }
        public virtual ICollection<Projekt> Projekt { get; set; }
        public virtual ICollection<Voditelj> Voditelj { get; set; }
    }
}
