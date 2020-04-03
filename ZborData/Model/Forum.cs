using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Forum
    {
        public Forum()
        {
            ModeratorForuma = new HashSet<ModeratorForuma>();
        }

        public Guid Id { get; set; }
        public string Naziv { get; set; }
        public Guid IdKategorijaForuma { get; set; }
        public virtual KategorijaForuma IdKategorijaForumaNavigation {get; set;}
        public virtual ICollection<ModeratorForuma> ModeratorForuma { get; set; }
    }
}
