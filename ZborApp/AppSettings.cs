using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZborApp
{
    public class AppSettings
    {

        public int PageSize { get; set; } = 2;
        public int PageOffset { get; set; } = 2;
        public string Secret { get; set; } = "Tajna je otkrivena hehe";


    }
}
