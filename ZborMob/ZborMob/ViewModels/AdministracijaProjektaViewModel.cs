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
    public class AdministracijaProjektaViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;

        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<string, ObservableCollection<ClanNaProjektu>> clanovi;

        private ObservableCollection<ClanNaProjektu> nerazvrstani;

        private ObservableCollection<PrijavaZaProjekt> prijave;
        private ObservableCollection<Korisnik> pretraga;
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

        public Dictionary<string, ObservableCollection<ClanNaProjektu>> Clanovi
        {
            get
            {
                return clanovi;
            }
            set
            {
                clanovi = value;
            }
        }
      
        public ObservableCollection<ClanNaProjektu> Nerazvrstani
        {
            get
            {
                return nerazvrstani;
            }
            set
            {
                nerazvrstani = value;
                RaisepropertyChanged("Nerazvrstani");

            }
        }
       
        public ObservableCollection<Korisnik> Pretraga
        {
            get
            {
                return pretraga;
            }
            set
            {
                pretraga = value;
                RaisepropertyChanged("Pretraga");

            }
        }
        public ObservableCollection<PrijavaZaProjekt> Prijave
        {
            get
            {
                return prijave;
            }
            set
            {
                prijave = value;
                RaisepropertyChanged("Prijave");

            }
        }
        public AdministracijaProjektaViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            GetData(id);

        }
        public string Uvjet { get; set; }
        public Projekt Projekt { get; set; }
        public bool Napunjeno { get; set; }
        async void GetData(Guid id)
        {
            IsBusy = true;
            //model = await _apiServices.PocetnaAsync();
            var model = await _apiServices.AdministracijaProjektaAsync(id);
            IsBusy = false;
            //var model = new ZborDataStandard.ViewModels.ZborViewModels.AdministracijaProjektaViewModel();
            Projekt = model.Projekt;
            Nerazvrstani = new ObservableCollection<ClanNaProjektu>(model.Nerazvrstani);
            Clanovi = new Dictionary<string, ObservableCollection<ClanNaProjektu>>();
            foreach(var kljuc in model.Clanovi.Keys)
            {
                Clanovi[kljuc] = new ObservableCollection<ClanNaProjektu>(model.Clanovi[kljuc]);
            }
            RaisepropertyChanged("Clanovi");
            Prijave = new ObservableCollection<PrijavaZaProjekt>(model.Projekt.PrijavaZaProjekt);
            Pretraga = new ObservableCollection<Korisnik>();
            Napunjeno = false;
        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public async void PrihvatiPrijavu(PrijavaZaProjekt prijava)
        {
            Prijave.Remove(prijava);
            var clan = await _apiServices.PrihvatiPrijavuProjektAsync(prijava);
            Nerazvrstani.Add(clan);
        }
        public async void ObrisiPrijavu(PrijavaZaProjekt prijava)
        {
            Prijave.Remove(prijava);
            await _apiServices.ObrisiPrijavuProjektAsync(prijava);
        }
       
        public async void Pretrazi()
        {
            var list = await _apiServices.PretraziProjektAsync(Projekt.Id,Uvjet);
            Pretraga = new ObservableCollection<Korisnik>(list);
        }
        public async void Obrisi()
        {
            await _apiServices.ObrisiProjektAsync(Projekt.Id);
        }
        public async void Zavrsi()
        {
            await _apiServices.ZavrsiProjektAsync(Projekt.Id);
        }
        public async void DodajClan(Korisnik korisnik)
        {
            Pretraga.Remove(korisnik);
            var clan = await _apiServices.DodajClanProjektAsync(korisnik.Id, Projekt.Id);
            Nerazvrstani.Add(clan);
        }
        public async void Izbaci(ClanNaProjektu clan)
        {

            var glas = clan.Uloga;
            if (glas.Equals("Nema"))
                Nerazvrstani.Remove(clan);
            else 
                Clanovi[glas].Remove(clan);
            await _apiServices.ObrisiClanProjektaAsync(clan.Id);
        }
        
        public async void PromjenaGlasa(ClanNaProjektu clan, string glas)
        {
            if (clan.Uloga.Equals("Nema"))
                Nerazvrstani.Remove(clan);
            else
                Clanovi[clan.Uloga].Remove(clan);
            clan.Uloga = glas;
            Clanovi[clan.Uloga].Add(clan);
            await _apiServices.PromjenaUlogeAsync(clan);
        }
    }
}
