using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ZborApp.Models.ZborViewModels
{
    public class AnketaModel
    {
        public string Json { get; set; }
        [Required(ErrorMessage = "Datum je obavezan.")]
        public DateTime DatumKraj { get; set; }
    }
}
