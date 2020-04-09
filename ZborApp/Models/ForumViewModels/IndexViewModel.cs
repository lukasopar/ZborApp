using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ForumViewModels
{
    public class IndexViewModel
    {
        public List<KategorijaForuma> KategorijaForuma { get; set; }
        public Forum Novi { get; set; }
        public bool Mod { get; set; }
        public bool Admin { get; set; }

    }
}
