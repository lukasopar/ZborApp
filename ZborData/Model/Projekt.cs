using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage= "Naziv projekta je obavezan")]
        public string Naziv { get; set; }
        [Required(ErrorMessage = "Opis projekta je obavezan")]
        public string Opis { get; set; }
        [Required(ErrorMessage = "Datum početka projekta je obavezan")]
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
