using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.ViewModels.JSONModels;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class KalendarViewModel
    {
        public Guid IdZbor { get; set; }
        public List<DogadjajModel> Dogadjaji { get; set; }
        //public string JsonString { get; set; }
    }
}
