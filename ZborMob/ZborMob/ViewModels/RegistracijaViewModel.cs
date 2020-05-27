using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZborDataStandard.ViewModels.AccountViewModels;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class RegistracijaViewModel
    {

        public RegisterViewModel model { get; set; }
        private ApiServices _apiServices = new ApiServices();
        public RegistracijaViewModel()
        {
            model = new RegisterViewModel();
            
        }
        public async Task<string> Reg()
        {
            return await _apiServices.RegisterAsync(model);
        }

    }
}
