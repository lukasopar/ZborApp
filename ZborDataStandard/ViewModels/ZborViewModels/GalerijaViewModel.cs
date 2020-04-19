using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ZborViewModels
{
    public class GalerijaViewModel
    {
        public  List<RepozitorijZbor> Slike { get; set; }
        public bool Clan { get; set; }
        public Guid IdZbor { get; set; }
    }
}
