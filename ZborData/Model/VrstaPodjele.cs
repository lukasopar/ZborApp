using System;
using System.Collections.Generic;
using System.Linq;

namespace ZborData.Model
{
    public partial class VrstaPodjele
    {
        public VrstaPodjele()
        {
            Projekt = new HashSet<Projekt>();
        }

        public Guid Id { get; set; }
        public string Podjela { get; set; }

        public virtual ICollection<Projekt> Projekt { get; set; }

        public List<string> Glasovi()
        {
            int sopr = Podjela.Count(c => c == 'S');
            int alt = Podjela.Count(c => c == 'A');
            int ten = Podjela.Count(c => c == 'T');
            int bas = Podjela.Count(c => c == 'B');
            List<string> glasovi = new List<string>();
            if (sopr > 1)
            {
                for (int i = 1; i <= sopr; i++)
                    glasovi.Add("Sopran " + i);
            }
            else if (sopr == 1)
                glasovi.Add("Sopran");

            if (alt > 1)
            {
                for (int i = 1; i <= alt; i++)
                    glasovi.Add("Alt " + i);
            }
            else if (alt == 1)
                glasovi.Add("Alt");

            if (ten > 1)
            {
                for (int i = 1; i <= ten; i++)
                    glasovi.Add("Tenor " + i);
            }
            else if (ten == 1)
                glasovi.Add("Tenor");

            if (bas > 1)
            {
                for (int i = 1; i <= bas; i++)
                    glasovi.Add("Bas " + i);
            }
            else if (bas == 1)
                glasovi.Add("Bas");
            return glasovi;
        }
    }
}
