using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Projekt
    {
        public Projekt()
        {
            ClanNaProjektu = new HashSet<ClanNaProjektu>();
            Dogadjaj = new HashSet<Dogadjaj>();
            ObavijestVezanaUzProjekt = new HashSet<ObavijestVezanaUzProjekt>();
            PozivZaProjekt = new HashSet<PozivZaProjekt>();
            PretplataNaProjekt = new HashSet<PretplataNaProjekt>();
            PrijavaZaProjekt = new HashSet<PrijavaZaProjekt>();
            Trosak = new HashSet<Trosak>();
        }

        public Guid Id { get; set; }
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public DateTime DatumPocetka { get; set; }
        public Guid IdZbor { get; set; }
        public Guid IdVrstePodjele { get; set; }

        public virtual VrstaPodjele IdVrstePodjeleNavigation { get; set; }
        public virtual Zbor IdZborNavigation { get; set; }
        public virtual ICollection<ClanNaProjektu> ClanNaProjektu { get; set; }
        public virtual ICollection<Dogadjaj> Dogadjaj { get; set; }
        public virtual ICollection<ObavijestVezanaUzProjekt> ObavijestVezanaUzProjekt { get; set; }
        public virtual ICollection<PozivZaProjekt> PozivZaProjekt { get; set; }
        public virtual ICollection<PretplataNaProjekt> PretplataNaProjekt { get; set; }
        public virtual ICollection<PrijavaZaProjekt> PrijavaZaProjekt { get; set; }
        public virtual ICollection<Trosak> Trosak { get; set; }
    }
}
