using System;
using System.Collections.Generic;
using System.Linq;

namespace ZborData.Model
{
    public partial class Forum
    {
        public Forum()
        {
            ModeratorForuma = new HashSet<ModeratorForuma>();
            Tema = new HashSet<Tema>();

        }

        public Guid Id { get; set; }
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public Guid IdKategorijaForuma { get; set; }
        public virtual KategorijaForuma IdKategorijaForumaNavigation {get; set;}
        public virtual ICollection<ModeratorForuma> ModeratorForuma { get; set; }
        public virtual ICollection<Tema> Tema { get; set; }

        public Tema ZadnjaTema()
        {
            return Tema.OrderByDescending(t => t.ZadnjiZapis).FirstOrDefault();
        }

    }
}
