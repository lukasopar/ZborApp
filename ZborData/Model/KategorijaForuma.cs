using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class KategorijaForuma
    {
        public KategorijaForuma()
        {
            Forum = new HashSet<Forum>();
        }
        public Guid Id { get; set; }
        public string Naziv { get; set; }
        public int Redoslijed { get; set; }
        public virtual ICollection<Forum> Forum { get; set; }


    }
}
