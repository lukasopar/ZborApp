using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ZborDataStandard.Model
{
    public partial class Forum
    {
        public Forum()
        {
            Tema = new HashSet<Tema>();

        }

        public Guid Id { get; set; }
        [Required(ErrorMessage = "Naziv je obavezan.")]
        public string Naziv { get; set; }
        [Required(ErrorMessage = "Opis je obavezan.")]
        public string Opis { get; set; }
        public Guid IdKategorijaForuma { get; set; }
        public virtual KategorijaForuma IdKategorijaForumaNavigation {get; set;}
        public virtual ICollection<Tema> Tema { get; set; }

        public Tema ZadnjaTema()
        {
            return Tema.OrderByDescending(t => t.ZadnjiZapis).FirstOrDefault();
        }

    }
}
