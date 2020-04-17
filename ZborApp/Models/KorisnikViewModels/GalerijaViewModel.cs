using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.KorisnikVIewModels
{
    public class GalerijaViewModel
    {
        public  List<RepozitorijKorisnik> Slike { get; set; }
        public bool Clan { get; set; }
        public Guid IdKorisnik { get; set; }
    }
}
