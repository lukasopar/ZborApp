using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ForumViewModels
{
    public class TemeViewModel
    {
        public List<Tema> Teme { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public Tema Nova { get; set; }
        [Required(ErrorMessage = "Zapis je obavezan")]
        public string Tekst { get; set; }
        public string Naslov { get; set; }
        public Guid IdForum { get; set; }
        public Guid IdBrisanje { get; set; }
        public Guid IdKorisnik { get; set; }
        public bool Mod { get; set; }
    }
}
