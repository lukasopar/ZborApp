﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
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

        public Korisnik Voditelj { get; set; }
        public bool Vod { get; set; }

    }
}
