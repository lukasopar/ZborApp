using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZborData.Model
{
    public partial class Zapis
    {
        public Zapis()
        {
        }

        public Guid Id { get; set; }
        public Guid IdKorisnik { get; set; }
        [Required(ErrorMessage="Zapis je obavezan")]
        public string Tekst { get; set; }
        public DateTime DatumIvrijeme { get; set; }
        public Guid IdTema { get; set; }
        public virtual Tema IdTemaNavigation { get; set; }
        public virtual Korisnik IdKorisnikNavigation { get; set; }
    }
}
