using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class AdministracijaProjektaViewModel
    {
        public Projekt Projekt { get; set; }
        public Dictionary<string, List<ClanNaProjektu>> Clanovi { get; set; } = new Dictionary<string, List<ClanNaProjektu>>();
        public List<ClanNaProjektu> Nerazvrstani { get; set; } = new List<ClanNaProjektu>();
        public Guid IdBrisanje { get; set; }
    }
}
