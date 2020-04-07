using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ForumViewModels
{
    public class ZapisVIewModel
    {
        public List<Zapis> Zapisi { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public Zapis Novi { get; set; }
        public Guid IdTema { get; set; }
        public Guid IdBrisanje { get; set; }
    }
}
