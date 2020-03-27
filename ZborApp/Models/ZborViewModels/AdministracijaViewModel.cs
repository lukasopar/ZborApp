using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class AdministracijaViewModel
    {
        public Zbor Zbor { get; set; }
        public ICollection<ClanZbora> Soprani { get; set; }
        public ICollection<ClanZbora> Alti { get; set; }

        public ICollection<ClanZbora> Tenori { get; set; }

        public ICollection<ClanZbora> Basi { get; set; }

        public ICollection<ClanZbora> Nerazvrstani { get; set; }
        public Guid IdBrisanje { get; set; }
        public ICollection<ModeratorZbora> Moderatori { get; set; }


    }
}
