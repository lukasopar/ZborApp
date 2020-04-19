using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class Trosak
    {
        public Guid Id { get; set; }
        public Guid IdProjekt { get; set; }
        public string Naslov { get; set; }
        public string Opis { get; set; }
        public double Cijena { get; set; }

        public virtual Projekt IdProjektNavigation { get; set; }
    }
}
