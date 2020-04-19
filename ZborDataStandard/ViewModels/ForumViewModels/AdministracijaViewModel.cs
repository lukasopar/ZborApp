using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ForumViewModels
{
    public class AdministracijaViewModel
    {
        public IList<AdministratorForuma> Administratori { get; set; }
        public IList<ModForum> Moderatori { get; set; }
        public Guid IdCilj { get; set; }
    }
}
