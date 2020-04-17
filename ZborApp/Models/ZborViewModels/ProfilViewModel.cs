using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZborData.Model;

namespace ZborApp.Models.ZborViewModels
{
    public class ProfilViewModel
    {
        public Guid IdKorisnik { get; set; }
        public Guid Slika { get; set; }
        public string ImeIPrezime { get; set; }
        public Zbor Zbor { get; set; }
        public Obavijest NovaObavijest { get; set; }
        public IEnumerable<Obavijest> Obavijesti { get; set; }
        public IList<Projekt> Projekti { get; set; }
        public string OdabraniProjekti { get; set; }
        public bool Admin { get; set; }

    }
}
