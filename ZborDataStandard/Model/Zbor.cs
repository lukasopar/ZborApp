﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZborDataStandard.Model
{
    public partial class Zbor : INotifyPropertyChanged
    {
        public Zbor()
        {
            Anketa = new HashSet<Anketa>();
            ClanZbora = new HashSet<ClanZbora>();
            ModeratorZbora = new HashSet<ModeratorZbora>();
            Obavijest = new HashSet<Obavijest>();
            PozivZaZbor = new HashSet<PozivZaZbor>();
            PrijavaZaZbor = new HashSet<PrijavaZaZbor>();
            PretplataNaZbor = new HashSet<PretplataNaZbor>();
            Projekt = new HashSet<Projekt>();
            RepozitorijZbor = new HashSet<RepozitorijZbor>();
            Voditelj = new HashSet<Voditelj>();
        }

        public Guid Id { get; set; }
        [Required(ErrorMessage = "Naziv zbora je obavezan.")]
        public string Naziv { get; set; }
        [Required(ErrorMessage = "Adresa zbora je obavezna.")]
        public string Adresa { get; set; }
        public string Opis { get; set; }
        [Required(ErrorMessage = "Datum osnutka zbora je obavezan.")]
        public DateTime DatumOsnutka { get; set; }
        public Guid IdSlika { get; set; }
        public RepozitorijZbor IdSlikaNavigation { get; set; }
        public virtual ICollection<Anketa> Anketa { get; set; }
        public virtual ICollection<ClanZbora> ClanZbora { get; set; }
        public virtual ICollection<ModeratorZbora> ModeratorZbora { get; set; }
        public virtual ICollection<Obavijest> Obavijest { get; set; }
        public virtual ICollection<PozivZaZbor> PozivZaZbor { get; set; }
        public virtual ICollection<PretplataNaZbor> PretplataNaZbor { get; set; }
        public virtual ICollection<PrijavaZaZbor> PrijavaZaZbor { get; set; }
        public virtual ICollection<Projekt> Projekt { get; set; }
        public virtual ICollection<RepozitorijZbor> RepozitorijZbor { get; set; }
        public virtual ProfilZbor ProfilZbor { get; set; }
        public virtual ICollection<Voditelj> Voditelj { get; set; }

        #region Procitano event za poruke
        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotMapped]
        public Guid SlikaProp
        {
            get
            {
                return IdSlika;
            }
            set
            {
                IdSlika = value;
                RaisepropertyChanged("SlikaProp");
            }
        }
        private void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
