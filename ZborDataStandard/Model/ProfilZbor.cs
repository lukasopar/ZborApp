using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class ProfilZbor
    {
        public Guid Id { get; set; }
  
        public string OZboru { get; set; }
        public string Repertoar { get; set; }
        public string OVoditeljima { get; set; }
        public string Reprezentacija { get; set; }
        public virtual Zbor IdZborNavigation { get; set; }
    }
}
