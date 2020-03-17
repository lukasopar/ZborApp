using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class OdgovorAnkete
    {
        public OdgovorAnkete()
        {
            OdgovorKorisnikaNaAnketu = new HashSet<OdgovorKorisnikaNaAnketu>();
        }

        public Guid Id { get; set; }
        public Guid IdAnketa { get; set; }
        public string Odgovor { get; set; }
        public int Redoslijed { get; set; }

        public virtual Anketa IdAnketaNavigation { get; set; }
        public virtual ICollection<OdgovorKorisnikaNaAnketu> OdgovorKorisnikaNaAnketu { get; set; }
    }
}
