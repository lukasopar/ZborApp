using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZborDataStandard.Model
{
    public partial class Razgovor : INotifyPropertyChanged
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

        #region Procitano event za poruke
        public event PropertyChangedEventHandler PropertyChanged;
        [NotMapped]
        private bool procitano;
        [NotMapped]
        public bool Procitano
        {
            get
            {
                return procitano;
            }
            set
            {
                procitano = value;
                RaisepropertyChanged("Procitano");
            }
        }
        private void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public string GetNaslov()
        {
            if ((Naslov == null || Naslov.Equals("")) && KorisnikUrazgovoru.Count > 2)
                return "Grupa";
            else if (Naslov != null && !Naslov.Equals(""))
                return Naslov;
            else return "Razgovor";
        }

        public string GetPopisKorisnika(Guid id)
        {
            string popis = "";
            foreach(var k in KorisnikUrazgovoru)
            {
                if (k.IdKorisnik == id)
                    continue;
                popis += k.IdKorisnikNavigation.ImeIPrezime() + ", ";
            }
            popis = popis.Remove(popis.Length - 2);
            return popis;
        }
        public string GetPopisKorisnikaEnter(Guid id)
        {
            string popis = "";
            foreach (var k in KorisnikUrazgovoru)
            {
                if (k.IdKorisnik == id)
                    continue;
                popis += k.IdKorisnikNavigation.ImeIPrezime() + "\n";
            }
            return popis;
        }


    }
}
