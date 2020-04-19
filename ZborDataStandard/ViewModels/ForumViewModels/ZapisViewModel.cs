using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborDataStandard.Model;

namespace ZborDataStandard.ViewModels.ForumViewModels
{
    public class ZapisVIewModel
    {
        public List<Zapis> Zapisi { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public Zapis Novi { get; set; }
        public Guid IdTema { get; set; }
        public Guid IdBrisanje { get; set; }
        public Guid IdKorisnik { get; set; }
        public string Naslov { get; set; }
        public bool Mod { get; set; }

    }
}
