using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZborDataStandard.ViewModels.JSONModels
{
    public class DogadjajModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; } = "#FFF";

    }
}
