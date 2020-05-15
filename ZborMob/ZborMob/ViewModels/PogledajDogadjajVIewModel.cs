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
        private bool isBusy;
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
        }
        private Guid id;
        private async void GetData(Guid id)
        {
            IsBusy = false;
            model = await _apiServices.DogadjajViewModelAsync(id);
            IsBusy = true;
            this.id = id;
        }
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void Spremi(List<Guid> guids)
        {
            var mod = new ZborDataStandard.ViewModels.ZborViewModels.DogadjajViewModel
            {
                IdDogadjaj = id,
                Evidencija = guids
            };
            model.Evidencija = guids;
            _apiServices.EvidentirajAsync(mod);
        }
    }
}
