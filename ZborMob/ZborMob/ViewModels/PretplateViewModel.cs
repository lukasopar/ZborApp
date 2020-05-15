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
using ZborMob.Model;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class PretplateViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<PretplataNaProjekt> pretplate;
        public ObservableCollection<PretplataNaProjekt> PretplataNaProjekt
        {
            get
            {
                return pretplate;
            }
            set
            {
                pretplate = value;
                RaisePropertyChanged("PretplataNaProjekt");
            }
        }
        
       private PretplataNaZbor pretZbor;
        public PretplataNaZbor PretplataNaZbor
        {
            get
            {
                return pretZbor;
            }
            set
            {
                pretZbor = value;
                RaisePropertyChanged("PretplataNaZbor");
            }
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
        public PretplateViewModel model { get; set; }
        public PretplateViewModel()
        {
            _apiServices = new ApiServices();
            IsBusy = true;
            GetData();

        }

        async void GetData()
        {
            //model = await _apiServices.PocetnaAsync();
            var model = await _apiServices.PretplateAsync(App.Zbor.Id);
            IsBusy = false;
            PretplataNaZbor = model.PretplataZbor;
            PretplataNaProjekt = new ObservableCollection<PretplataNaProjekt>(model.PretplataProjekt);
        }
        public async void Spremi()
        {
            await _apiServices.PretplateSpremiAsync(App.Zbor.Id, new ZborDataStandard.ViewModels.ZborViewModels.PretplateViewModel { PretplataZbor = PretplataNaZbor, PretplataProjekt = PretplataNaProjekt.ToList() });
        }
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
