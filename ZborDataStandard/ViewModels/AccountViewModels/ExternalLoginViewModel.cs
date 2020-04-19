using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ZborDataStandard.ViewModels.AccountViewModels
{
    public class ExternalLoginViewModel
    {

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Adresa { get; set; }
        public string Grad { get; set; }
        public string PostanskiBroj { get; set; }
        public string SlikaUrl { get; set; }
        public string DatumRodjenja { get; set; }
        public string OMeni { get; set; }
    }
}
