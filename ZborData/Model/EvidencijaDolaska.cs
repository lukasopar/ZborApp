﻿using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class EvidencijaDolaska
    {
        public Guid Id { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdDogadjaj { get; set; }

        public virtual Dogadjaj IdDogadjajNavigation { get; set; }
        public virtual Korisnik IdKorisnikNavigation { get; set; }
    }
}
