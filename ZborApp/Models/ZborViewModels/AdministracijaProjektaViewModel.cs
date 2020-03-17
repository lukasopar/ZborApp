using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class AdministracijaProjektaViewModel
    {
        public Projekt Projekt { get; set; }
        public ICollection<ClanNaProjektu> Clanovi { get; set; }


    }
}
