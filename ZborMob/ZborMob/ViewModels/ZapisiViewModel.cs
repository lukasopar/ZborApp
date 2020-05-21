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
using ZborDataStandard.ViewModels.ForumViewModels;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class ZapisiViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Zapis> zapisi;
        public ObservableCollection<Zapis> Zapisi
        {
            get
            {
                return zapisi;
            }
            set
            {
                zapisi = value;
                RaisepropertyChanged("Zapisi");
            }
        }
        private Zapis novi;
        public Zapis Novi
        {
            get
            {
                return novi;
            }
            set
            {
                novi = value;
                RaisepropertyChanged("Novi");
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

        private bool mod;
        public bool Mod
        {
            get
            {
                return mod;
            }
            set
            {
                mod = value;
                RaisepropertyChanged("Mod");
            }
        }

        private int stranica;
        public int Stranica
        {
            get
            {
                return stranica;
            }
            set
            {
                stranica = value;
                RaisepropertyChanged("Stranica");
            }
        }
        private string naslov;
        public string Naslov
        {
            get
            {
                return naslov;
            }
            set
            {
                naslov = value;
                RaisepropertyChanged("Naslov");
            }
        }
        private ZborDataStandard.ViewModels.ForumViewModels.ZapisVIewModel model;
        private Guid idPodforum;
        public ZapisiViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            IsBusy = true;
            GetData(id);
            

        }
        async void GetData(Guid id)
        {
            model = await _apiServices.Zapisi(id);
            idPodforum = id;
            Novi = new Zapis();
            IsBusy = false;
            Naslov = model.Naslov;
            Mod = model.Mod;
            Zapisi = new ObservableCollection<Zapis>(model.Zapisi);
            Stranica = model.PagingInfo.TotalPages;
        }
        public async Task Promjena(int stranica)
        {
            Zapisi.Clear();
            IsBusy = true;
            model = await _apiServices.Zapisi(idPodforum, stranica);
            IsBusy = false;
            Zapisi = new ObservableCollection<Zapis>(model.Zapisi);
            Stranica = model.PagingInfo.TotalPages;
            return;
        }
        public async Task Dodaj()
        {
            Novi.IdTema = idPodforum;
            Novi.IdKorisnik = App.Korisnik.Id;
            await _apiServices.NoviZapis(Novi);
            Novi = new Zapis();
            await Promjena(0);
            
        }
        public async Task Spremi(string tekst, Zapis zapis)
        {
            await _apiServices.UrediZapis(zapis.Id, tekst);

        }
        public async Task ObrisiZapis(Zapis zapis, int stranica)
        {
            await _apiServices.ObrisiZapis(zapis.Id);
            await Promjena(stranica);
        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
