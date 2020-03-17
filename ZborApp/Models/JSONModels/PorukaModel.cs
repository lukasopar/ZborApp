using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.JSONModels
{
    public class PorukaModel
    {
        public string IdRazg { get; set; }
        public string IdUser { get; set; }
        public string Message { get; set; }
        public DateTime When { get; set; }
        public string Slika { get; set; }
        public string Ime { get; set; }
    }
}
