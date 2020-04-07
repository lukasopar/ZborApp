using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ForumViewModels
{
    public class AdministracijaViewModel
    {
        public IList<AdministratorForuma> Administratori { get; set; }
        public IList<ModForum> Moderatori { get; set; }
        public Guid IdCilj { get; set; }
    }
}
