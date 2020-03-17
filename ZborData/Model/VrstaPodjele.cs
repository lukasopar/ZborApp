using System;
using System.Collections.Generic;

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
    }
}
