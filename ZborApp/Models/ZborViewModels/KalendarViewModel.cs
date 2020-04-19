using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborApp.Models.JSONModels;
using ZborDataStandard.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class KalendarViewModel
    {
        public Guid IdZbor { get; set; }
        public List<DogadjajModel> Dogadjaji { get; set; }
        //public string JsonString { get; set; }
    }
}
