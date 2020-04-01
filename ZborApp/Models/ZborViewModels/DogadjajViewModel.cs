using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class DogadjajViewModel
    {
        public Dogadjaj Dogadjaj { get; set; }

        public Dictionary<string, List<NajavaDolaska>> Clanovi { get; set; } = new Dictionary<string, List<NajavaDolaska>>();
        public List<NajavaDolaska> Nerazvrstani { get; set; } = new List<NajavaDolaska>();
        public Dictionary<string, List<ClanNaProjektu>> ClanoviProjekta { get; set; } = new Dictionary<string, List<ClanNaProjektu>>();
        public List<ClanNaProjektu> NerazvrstaniClanovi { get; set; } = new List<ClanNaProjektu>();
        public List<Guid> Evidencija { get; set; }

        public Guid IdDogadjaj { get; set; }


    }
}
