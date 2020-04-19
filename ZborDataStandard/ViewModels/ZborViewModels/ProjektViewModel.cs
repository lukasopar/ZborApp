using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class ProjektViewModel
    {
        public Projekt Projekt { get; set; }
        public IEnumerable<Dogadjaj> AktivniDogadjaji { get; set; }
        public IEnumerable<Dogadjaj> ProsliDogadjaji { get; set; }

        public IEnumerable<Obavijest> Obavijesti { get; set; }
        public Guid IdKorisnik { get; set; }
        public bool Admin { get; set; }
        public bool Clan { get; set; }
        public Guid Slika { get; set; }
        public string ImeIPrezime { get; set; }
    }
}
