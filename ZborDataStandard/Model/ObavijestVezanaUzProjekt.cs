using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class ObavijestVezanaUzProjekt
    {
        public Guid Id { get; set; }
        public Guid IdProjekt { get; set; }
        public Guid IdObavijest { get; set; }

        public virtual Obavijest IdObavijestNavigation { get; set; }
        public virtual Projekt IdProjektNavigation { get; set; }
    }
}
