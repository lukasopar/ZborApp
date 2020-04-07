using System;
using System.Collections.Generic;
using ZborData.Services;

namespace ZborData.Model
{
    public partial class RepozitorijZbor { 
        public Guid Id { get; set; }
        public string Naziv { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdZbor { get; set; }
        public string Url { get; set; }
        public DateTime DatumPostavljanja { get; set; }
        public bool Privatno { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Zbor IdZborNavigation { get; set; }


        public string GetSlika()
        {
            var vrsta = Naziv.Split(".");
            var ekstenzija = vrsta[vrsta.Length - 1].ToLower().Trim();
            return ExstensionDictionary.GetLink(ekstenzija);
        }
        public string GetEkstenzija()
        {
            var vrsta = Naziv.Split(".");
            var ekstenzija = vrsta[vrsta.Length - 1].ToLower().Trim();
            return ekstenzija;
        }
        public string GetNaziv()
        {
            var vrsta = Naziv.Split(".");
            string naziv = "";
            for (int i = 0; i < vrsta.Length - 1; i++)
                naziv += vrsta[i] + ".";
            naziv = naziv.Remove(naziv.Length - 1);
            return naziv;
        }
    }
}
