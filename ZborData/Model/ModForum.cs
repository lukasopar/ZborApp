﻿using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class ModForum
    {
        public Guid Id { get; set; }
  
        public virtual Korisnik IdKorisnikNavigation { get; set; }
    }
}
