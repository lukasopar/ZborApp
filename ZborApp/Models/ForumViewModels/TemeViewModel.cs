using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ForumViewModels
{
    public class TemeViewModel
    {
        public List<Tema> Teme { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public Tema Nova { get; set; }
        public string Tekst { get; set; }
        public Guid IdForum { get; set; }
        public Guid IdBrisanje { get; set; }
    }
}
