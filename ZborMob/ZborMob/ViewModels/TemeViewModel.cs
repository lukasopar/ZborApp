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
    public class TemeViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Tema> teme;
        public ObservableCollection<Tema> Teme
        {
            get
            {
                return teme;
            }
            set
            {
                teme = value;
                RaisepropertyChanged("Teme");
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


        private string naslovNovi;
        public string NaslovNovi
        {
            get
            {
                return naslovNovi;
            }
            set
            {
                naslovNovi = value;
                RaisepropertyChanged("NaslovNovi");
            }
        }
        private string tekst;
        public string Tekst
        {
            get
            {
                return tekst;
            }
            set
            {
                tekst = value;
                RaisepropertyChanged("Tekst");
            }
        }


        private ZborDataStandard.ViewModels.ForumViewModels.TemeViewModel model;
        private Guid idPodforum;
        public TemeViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            IsBusy = true;
            GetData(id);

        }
        async void GetData(Guid id)
        {
            model = await _apiServices.Teme(id);
            idPodforum = id;
            IsBusy = false;
            Naslov = model.Naslov;
            Mod = model.Mod;
            Teme = new ObservableCollection<Tema>(model.Teme);
            Stranica = model.PagingInfo.TotalPages;
        }
        public async Task Promjena(int stranica)
        {
            Teme.Clear();
            IsBusy = true;
            model = await _apiServices.Teme(idPodforum, stranica);
            IsBusy = false;
            Teme = new ObservableCollection<Tema>(model.Teme);
            Stranica = model.PagingInfo.TotalPages;
        }
        public async Task Dodaj()
        {
            var novi = new Tema();
            novi.IdForum = idPodforum;
            novi.IdKorisnik = App.Korisnik.Id;
            novi.Naslov = NaslovNovi;
            await _apiServices.NovaTema(novi, Tekst);
            await Promjena(1);

        }
        public async Task ObrisiTema(Tema tema, int stranica)
        {
            await _apiServices.ObrisiTema(tema.Id);
            await Promjena(stranica);
        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
