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
    public class AdministracijaViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ClanZbora> soprani;

        private ObservableCollection<ClanZbora> alti;

        private ObservableCollection<ClanZbora> tenori;

        private ObservableCollection<ClanZbora> basi;
        private ObservableCollection<ClanZbora> nerazvrstani;
        private ObservableCollection<ModeratorZbora> moderatori;
        private ObservableCollection<PozivZaZbor> pozivi;
        private ObservableCollection<PrijavaZaZbor> prijave;
        private ObservableCollection<Korisnik> pretraga;

        private Korisnik Voditelj;

        public ObservableCollection<ClanZbora> Soprani
        {
            get
            {
                return soprani;
            }
            set
            {
                soprani = value;
                RaisepropertyChanged("Soprani");
            }
        }
        public ObservableCollection<ClanZbora> Alti
        {
            get
            {
                return alti;
            }
            set
            {
                alti = value;
                RaisepropertyChanged("Alti");
            }
        }
        public ObservableCollection<ClanZbora> Tenori
        {
            get
            {
                return tenori;
            }
            set
            {
                tenori = value;
                RaisepropertyChanged("Tenori");

            }
        }
        public ObservableCollection<ClanZbora> Basi
        {
            get
            {
                return basi;
            }
            set
            {
                basi = value;
                RaisepropertyChanged("Basi");

            }
        }
        public ObservableCollection<ClanZbora> Nerazvrstani
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
        public ObservableCollection<ModeratorZbora> Moderatori
        {
            get
            {
                return moderatori;
            }
            set
            {
                moderatori = value;
                RaisepropertyChanged("Moderatori");

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
        public ObservableCollection<PrijavaZaZbor> Prijave
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
        public ObservableCollection<PozivZaZbor> Pozivi
        {
            get
            {
                return pozivi;
            }
            set
            {
                pozivi = value;
                RaisepropertyChanged("Pozivi");

            }
        }
        public AdministracijaViewModel()
        {
            _apiServices = new ApiServices();
            GetData();

        }
        public string Uvjet { get; set; }
        async void GetData()
        {
            //model = await _apiServices.PocetnaAsync();
            var model = await _apiServices.AdministracijaAsync(App.Zbor.Id);
            Soprani = new ObservableCollection<ClanZbora>(model.Soprani);
            Alti = new ObservableCollection<ClanZbora>(model.Alti);
            Tenori = new ObservableCollection<ClanZbora>(model.Tenori);
            Basi = new ObservableCollection<ClanZbora>(model.Basi);
            Nerazvrstani = new ObservableCollection<ClanZbora>(model.Nerazvrstani);
            Moderatori = new ObservableCollection<ModeratorZbora>(model.Zbor.ModeratorZbora);
            Voditelj = model.Voditelj;
            Prijave = new ObservableCollection<PrijavaZaZbor>(model.Zbor.PrijavaZaZbor);
            Pozivi = new ObservableCollection<PozivZaZbor>(model.Zbor.PozivZaZbor);
            Pretraga = new ObservableCollection<Korisnik>();
        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public async void PrihvatiPrijavu(PrijavaZaZbor prijava)
        {
            Prijave.Remove(prijava);
            var clan = await _apiServices.PrihvatiPrijavuZborAsync(prijava);
            Nerazvrstani.Add(clan);
        }
        public async void ObrisiPrijavu(PrijavaZaZbor prijava)
        {
            Prijave.Remove(prijava);
            await _apiServices.ObrisiPrijavuZborAsync(prijava);
        }
        public async void ObrisiPoziv(PozivZaZbor poziv)
        {
            Pozivi.Remove(poziv);
            await _apiServices.ObrisiPozivZborAsync(poziv);
        }
        public async void Pretrazi()
        {
            var list = await _apiServices.PretraziAsync(App.Zbor.Id,Uvjet);
            Pretraga = new ObservableCollection<Korisnik>(list);
        }
        public async void Pozovi(Korisnik korisnik)
        {
            Pretraga.Remove(korisnik);
            var poziv = await _apiServices.PozivZborAsync(korisnik.Id, App.Zbor.Id);
            Pozivi.Add(poziv);
        }
        public async void Izbaci(ClanZbora clan)
        {
            if (clan.IdKorisnik == App.Korisnik.Id)
                return;
            var glas = clan.Glas.ToLower();
            if (glas.Equals("sopran"))
                Soprani.Remove(clan);
            else if (glas.Equals("alt"))
                Alti.Remove(clan);
            else if (glas.Equals("tenor"))
                Tenori.Remove(clan);
            else if (glas.Equals("bas"))
                Basi.Remove(clan);
            else 
                Nerazvrstani.Remove(clan);
            await _apiServices.ObrisiClanZboraAsync(clan.Id);
        }
        public async void PostaviModeratora(ClanZbora clan)
        {
            if (Moderatori.Select(m => m.IdKorisnik).Contains(clan.IdKorisnik))
                return;
            var mod = await _apiServices.PostaviModeratoraAsync(clan.IdKorisnik, App.Zbor.Id);
            Moderatori.Add(mod);

        }
        public async void ObrisiModeratora(ModeratorZbora mod)
        {
            Moderatori.Remove(mod);
            await _apiServices.ObrisiModeratoraAsync(mod.Id);
        }
        public async void PostaviVoditelja(ClanZbora clan)
        {
            await _apiServices.PostaviVoditeljaAsync(clan.Id);
            Voditelj = clan.IdKorisnikNavigation;
        }
        public async void PromjenaGlasa(ClanZbora clan, int stariIndex, int noviIndex)
        {
            if (stariIndex == noviIndex)
                return;
            if (Nerazvrstani.Contains(clan))
                Nerazvrstani.Remove(clan);
            else if(stariIndex == 0)
                Soprani.Remove(clan);
            else if (stariIndex == 1)
                Alti.Remove(clan);
            else if (stariIndex == 2)
                Tenori.Remove(clan);
            else if (stariIndex == 3)
                Basi.Remove(clan);

            if (noviIndex == 0)
            { 
                Soprani.Add(clan);
                clan.Glas = "sopran";
            }
            else if (noviIndex == 1)
            {
                Alti.Add(clan);
                clan.Glas = "alt";
            }
            else if (noviIndex == 2)
            {
                Tenori.Add(clan);
                clan.Glas = "tenor";
            }
            else if (noviIndex == 3)
            {
                Basi.Add(clan);
                clan.Glas = "bas";
            }

            await _apiServices.PromjenaGlasaAsync(clan.Id, noviIndex+1);
        }
    }
}
