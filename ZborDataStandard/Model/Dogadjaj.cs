using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZborDataStandard.Model
{
    public partial class Dogadjaj
    {
        public Dogadjaj()
        {
            EvidencijaDolaska = new HashSet<EvidencijaDolaska>();
            NajavaDolaska = new HashSet<NajavaDolaska>();
        }

        public Guid Id { get; set; }
        public Guid IdVrsteDogadjaja { get; set; }
        public Guid IdProjekt { get; set; }
        [Required(ErrorMessage = "Obavezno polje")]
        public string DodatanOpis { get; set; }
        [Required(ErrorMessage="Obavezno polje")]
        public string Lokacija { get; set; }
        [Required(ErrorMessage = "Obavezno polje")]
        public string Naziv { get; set; }
        [Required(ErrorMessage = "Obavezno polje")]
        public DateTime DatumIvrijeme { get; set; }
        [Required(ErrorMessage = "Obavezno polje")]
        public DateTime DatumIvrijemeKraja { get; set; }


        public virtual VrstaDogadjaja IdProjekt1 { get; set; }
        public virtual Projekt IdProjektNavigation { get; set; }
        public virtual ICollection<EvidencijaDolaska> EvidencijaDolaska { get; set; }
        public virtual ICollection<NajavaDolaska> NajavaDolaska { get; set; }
        public string Trajanje()
        {
            if (DatumIvrijeme.Date == DatumIvrijemeKraja.Date)
                return DatumIvrijeme.ToString("dd.MM.yyyy. HH:mm") + " - " + DatumIvrijemeKraja.ToString("HH:mm");
            return DatumIvrijeme.ToString("dd.MM.yyyy. HH:mm") + " - " + DatumIvrijemeKraja.ToString("dd.MM.yyyy. - HH:mm");
        }
    }
}
