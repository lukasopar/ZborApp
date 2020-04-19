using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class ProjektiViewModel
    {
        public Projekt Novi { get; set; }
        public Guid IdZbor { get; set; }
        public Guid IdKorisnik { get; set; }
        public bool Admin { get; set; }

        public IEnumerable<Projekt> Projekti { get; set; }
        public IEnumerable<Projekt> MojiProjekti { get; set; }

        public IEnumerable<Projekt> PrijavaProjekti { get; set; }
        public IEnumerable<Projekt> OstaliProjekti { get; set; }
        public IEnumerable<Projekt> ZavrseniProjekti { get; set; }


        public List<SelectListItem> VrstePodjele { get; set; }  

    }
}
