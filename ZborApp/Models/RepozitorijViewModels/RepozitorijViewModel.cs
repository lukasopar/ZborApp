using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.RepozitorijViewModels
{
    public class RepozitorijZborViewModel
    {
        public IList<RepozitorijZbor> Datoteke { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdZbor { get; set; }

        public Guid IdTrazeni { get; set; }
        public IList<IFormFile> FormFiles { get; set; }
        public bool Promjena { get; set; }
        public bool Clan { get; set; }
    }
}
