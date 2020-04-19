using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class PretplateViewModel
    {
        public Zbor Zbor { get; set; }
        public PretplataNaZbor PretplataZbor { get; set; }
        public List<PretplataNaProjekt> PretplataProjekt { get; set; }
        public List<Projekt> Projekti { get; set; }

    }
}
