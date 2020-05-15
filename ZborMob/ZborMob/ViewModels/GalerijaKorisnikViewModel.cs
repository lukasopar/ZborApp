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
    public class GalerijaKorisnikViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<RepozitorijKorisnik> slike;
        public ObservableCollection<RepozitorijKorisnik> Slike
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
        public GalerijaKorisnikViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            GetData(id);

        }

        async void GetData(Guid id)
        {
            //model = await _apiServices.PocetnaAsync();
            IsBusy = true;
            var model = await _apiServices.GalerijaKorisnik(id);
            IsBusy = false;
            Slike = new ObservableCollection<RepozitorijKorisnik>(model.Slike);
        }
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public async void Profilna(Guid id)
        {
            await _apiServices.ProfilnaKorisnikAsync(id);

        }
    }
}
