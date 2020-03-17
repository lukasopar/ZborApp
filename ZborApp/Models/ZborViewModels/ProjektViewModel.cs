using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class ProjektViewModel
    {
        public Projekt Projekt { get; set; }
        public IEnumerable<Dogadjaj> AktivniDogadjaji { get; set; }
        public IEnumerable<Dogadjaj> ProsliDogadjaji { get; set; }


        public bool Admin { get; set; }
        

    }
}
