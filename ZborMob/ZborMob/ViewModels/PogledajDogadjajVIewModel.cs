using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.ZborViewModels;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class PogledajDogadjajViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;


        public event PropertyChangedEventHandler PropertyChanged;
        public ZborDataStandard.ViewModels.ZborViewModels.DogadjajViewModel model;
     
      
        public PogledajDogadjajViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            GetData(id);
            
        }
        private async void GetData(Guid id)
        {
            model = await _apiServices.DogadjajViewModelAsync(id);
        }
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
