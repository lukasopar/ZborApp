using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
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
