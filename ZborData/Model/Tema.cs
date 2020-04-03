using System;
using System.Collections.Generic;
using System.Linq;

namespace ZborData.Model
{
    public partial class Tema
    {
        public Tema()
        {
            Zapis = new HashSet<Zapis>();
        }
        public Guid Id { get; set; }
        public string Naslov { get; set; }
        public DateTime DatumPocetka { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdForum { get; set; }
        public DateTime ZadnjiZapis { get; set; }
        public virtual Forum IdForumNavigation { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual ICollection<Zapis> Zapis { get; set; }

        public Zapis Zadnji()
        {
            return Zapis.OrderByDescending(t => t.DatumIvrijeme).FirstOrDefault();
        }
    }
}
