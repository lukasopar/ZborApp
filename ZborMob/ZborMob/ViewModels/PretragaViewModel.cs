using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.ZborViewModels;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class PretragaViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Zbor> zborovi;
        private ObservableCollection<Korisnik> korisnici;

        public string Upit { get; set; }
        public ObservableCollection<Zbor> Zborovi
        {
            get
            {
                return zborovi;
            }
            set
            {
                zborovi = value;
                RaisepropertyChanged("Zborovi");
            }
        }
        public ObservableCollection<Korisnik> Korisnici
        {
            get
            {
                return korisnici;
            }
            set
            {
                korisnici = value;
                RaisepropertyChanged("Korisnici");
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
                RaisepropertyChanged("IsBusy");
            }
        }

       

        public PretragaViewModel()
        {
            _apiServices = new ApiServices();
            Zborovi = new ObservableCollection<Zbor>();
            Korisnici = new ObservableCollection<Korisnik>();

        }
        public async Task Pretraga()
        {
            IsBusy = true;
            var model = await _apiServices.Pretraga(Upit);
            IsBusy = false;
            Zborovi = new ObservableCollection<Zbor>(model.Zborovi);
            Korisnici = new ObservableCollection<Korisnik>(model.Korisnici);
        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
