using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.AccountViewModels;
using ZborDataStandard.ViewModels.ZborViewModels;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class DodajZborViewModel
    {

        public DodajViewModel model { get; set; }
        private ApiServices _apiServices = new ApiServices();
        public DodajZborViewModel()
        {
            model = new DodajViewModel();
            model.Novi = new Zbor();
        }
        public async Task<Zbor> Dodaj()
        {
            return await _apiServices.DodajZbor(model);
        }

    }
}
