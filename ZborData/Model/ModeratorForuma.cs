using System;
using System.Collections.Generic;

namespace ZborData.Model
{
    public partial class ModeratorForuma
    {
        public Guid Id { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdForum { get; set; }

        public virtual Forum IdForumNavigation { get; set; }
        public virtual Korisnik IdKorisnikNavigation { get; set; }
    }
}
