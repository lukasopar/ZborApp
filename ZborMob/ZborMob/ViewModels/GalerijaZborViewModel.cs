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
    public class GalerijaZborViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<RepozitorijZbor> slike;
        public ObservableCollection<RepozitorijZbor> Slike
        {
            get
            {
                return slike;
            }
            set
            {
                slike = value;
                RaisePropertyChanged("Slike");
            }
        }
        
        public GalerijaZborViewModel(Guid id)
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
        async void GetData(Guid id)
        {
            //model = await _apiServices.PocetnaAsync();
            IsBusy = true;
            var model = await _apiServices.GalerijaZbor(id);
            IsBusy = false;
            Slike = new ObservableCollection<RepozitorijZbor>(model.Slike);
        }
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public async void Profilna(Guid id)
        {
            await _apiServices.ProfilnaZborAsync(id);

        }
    }
}
