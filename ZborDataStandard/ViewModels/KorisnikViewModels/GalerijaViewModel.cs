using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.KorisnikVIewModels
{
    public class GalerijaViewModel
    {
        public  List<RepozitorijKorisnik> Slike { get; set; }
        public bool Clan { get; set; }
        public Guid IdKorisnik { get; set; }
    }
}
