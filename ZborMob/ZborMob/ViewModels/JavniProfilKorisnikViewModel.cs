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
    public class JavniProfilKorisnikViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;

        public event PropertyChangedEventHandler PropertyChanged;
        private string oMeni;
        public string OMeni { get { return oMeni; } set { oMeni = value; RaisePropertyChanged("OMeni"); } }
        public ObservableCollection<ClanZbora> zbor;
        public ObservableCollection<ClanZbora> Zbor
        {
            get
            {
                return zbor;
            }
            set
            {
                zbor = value;
                RaisePropertyChanged("Zbor");
            }
        }

        private ZborDataStandard.ViewModels.KorisnikVIewModels.JavniProfilViewModel model;
        public ZborDataStandard.ViewModels.KorisnikVIewModels.JavniProfilViewModel Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
                RaisePropertyChanged("Model");
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
        private bool clan;
        public bool Clan
        {
            get
            {
                return clan;
            }
            set
            {
                clan = value;
                RaisePropertyChanged("Clan");
            }
        }
        public JavniProfilKorisnikViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            OMeni = "";
            IsBusy = true;
            GetData(id);

        }
        private async void GetData(Guid id)
        {
            Model = await _apiServices.JavniProfilKorisnikAsync(id);
            IsBusy = false;
            OMeni = Model.Korisnik.Opis;
            Zbor = new ObservableCollection<ClanZbora>(model.Korisnik.ClanZbora);
        }
        public async void SpremiOMeni(string tekst)
        {
            await _apiServices.Spremiomeni(tekst);
        }
      
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
