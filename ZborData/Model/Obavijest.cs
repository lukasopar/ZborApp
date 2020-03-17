﻿using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class Obavijest
    {
        public Obavijest()
        {
            KomentarObavijesti = new HashSet<KomentarObavijesti>();
            LajkObavijesti = new HashSet<LajkObavijesti>();
            ObavijestVezanaUzProjekt = new HashSet<ObavijestVezanaUzProjekt>();
        }

        public Guid Id { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdZbor { get; set; }
        public string Tekst { get; set; }
        public DateTime DatumObjave { get; set; }
        public string Naslov { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Zbor IdZborNavigation { get; set; }
        public virtual ICollection<KomentarObavijesti> KomentarObavijesti { get; set; }
        public virtual ICollection<LajkObavijesti> LajkObavijesti { get; set; }
        public virtual ICollection<ObavijestVezanaUzProjekt> ObavijestVezanaUzProjekt { get; set; }
    }
}
