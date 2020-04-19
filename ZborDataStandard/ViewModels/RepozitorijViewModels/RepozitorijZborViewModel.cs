using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.RepozitorijViewModels
{
    public class RepozitorijViewModel
    {
        public IList<RepozitorijKorisnik> Datoteke { get; set; }
        public Guid IdKorisnik { get; set; }
        public Guid IdTrazeni { get; set; }
        public IList<IFormFile> FormFiles { get; set; }
    }
}
