using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class NoviDogadjajViewModel
    {
        public Guid IdProjekt { get; set; }
        public Dogadjaj Novi { get; set; }
        public List<SelectListItem> VrsteDogadjaja { get; set; }


    }
}
