using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ZborDataStandard.ViewModels.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Email je obavezan")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Lozinka je obavezna")]
        [StringLength(100, ErrorMessage = "Lozinka mora biti barem {2} i najviše {1} znakova duga.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Lozinke nisu iste!")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Ime je obavezno")]
        public string Ime { get; set; }
        [Required(ErrorMessage = "Prezme je obavezno")]
        public string Prezime { get; set; }
        [Required(ErrorMessage = "Datum rođenja je obavezan")]
        public string DatumRodjenja { get; set; }
        public string Opis { get; set; }
    }
}
