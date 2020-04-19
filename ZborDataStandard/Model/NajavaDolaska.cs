using System;
using System.Collections.Generic;

namespace ZborDataStandard.Model
{
    public partial class NajavaDolaska
    {
        public Guid Id { get; set; }
        public Guid IdDogadjaj { get; set; }
        public Guid IdKorisnik { get; set; }

        public virtual Dogadjaj IdDogadjajNavigation { get; set; }
        public virtual Korisnik IdKorisnikNavigation { get; set; }
    }
}
