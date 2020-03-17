using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZborApp.Models.JSONModels
{
    
    public class PitanjeJSON
    {
        public string pitanje { get; set; }
        public string vrsta { get; set; }
        public List<string> odgovori { get; set; }
    }
    public class OdgovoriJSON
    {
        public List<OdgovorJSON> odgovori { get; set; }

    }
    public class OdgovorJSON
    {
        public int questionId { get; set; }
        public string odgovor { get; set; }
    }


}
