using System;
using System.Collections.Generic;

namespace ZborData.Model
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
        public string DodatanOpis { get; set; }
        public string Lokacija { get; set; }
        public string Naziv { get; set; }
        public DateTime DatumIvrijeme { get; set; }
        public DateTime DatumIvrijemeKraja { get; set; }


        public virtual VrstaDogadjaja IdProjekt1 { get; set; }
        public virtual Projekt IdProjektNavigation { get; set; }
        public virtual ICollection<EvidencijaDolaska> EvidencijaDolaska { get; set; }
        public virtual ICollection<NajavaDolaska> NajavaDolaska { get; set; }
    }
}
