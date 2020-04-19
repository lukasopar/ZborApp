using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZborDataStandard.Model
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
        [Required]
        public string Tekst { get; set; }
        public DateTime DatumObjave { get; set; }
        [Required]
        public string Naslov { get; set; }

        public virtual Korisnik IdKorisnikNavigation { get; set; }
        public virtual Zbor IdZborNavigation { get; set; }
        public virtual ICollection<KomentarObavijesti> KomentarObavijesti { get; set; }
        public virtual ICollection<LajkObavijesti> LajkObavijesti { get; set; }
        public virtual ICollection<ObavijestVezanaUzProjekt> ObavijestVezanaUzProjekt { get; set; }
    }
}
