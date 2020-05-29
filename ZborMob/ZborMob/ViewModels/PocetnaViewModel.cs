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
    public class PocetnaViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Zbor> mojiZborovi;

        private ObservableCollection<Zbor> ostaliZborovi;

        private ObservableCollection<Zbor> prijaveZborovi;

        private ObservableCollection<Zbor> poziviZborovi;

        public ObservableCollection<Zbor> MojiZborovi
        {
            get
            {
                return mojiZborovi;
            }
            set
            {
                mojiZborovi = value;
                RaisepropertyChanged("MojiZborovi");
            }
        }
        public ObservableCollection<Zbor> OstaliZborovi
        {
            get
            {
                return ostaliZborovi;
            }
            set
            {
                ostaliZborovi = value;
                RaisepropertyChanged("OstaliZborovi");
            }
        }
        public ObservableCollection<Zbor> PrijaveZborovi
        {
            get
            {
                return prijaveZborovi;
            }
            set
            {
                prijaveZborovi = value;
                RaisepropertyChanged("PrijaveZborovi");

            }
        }
        public ObservableCollection<Zbor> PoziviZborovi
        {
            get
            {
                return poziviZborovi;
            }
            set
            {
                poziviZborovi = value;
                RaisepropertyChanged("PoziviZborovi");

            }
        }
        private IndexViewModel model;

        public PocetnaViewModel()
        {
            _apiServices = new ApiServices();
            GetData();

        }
        async void GetData()
        {
            model = await _apiServices.PocetnaAsync();
            MojiZborovi = new ObservableCollection<Zbor>(model.MojiZborovi);
            OstaliZborovi = new ObservableCollection<Zbor>(model.OstaliZborovi);
            PoziviZborovi = new ObservableCollection<Zbor>(model.MojiPozivi.Select(prijaveZborovi => prijaveZborovi.IdZborNavigation));

            PrijaveZborovi = new ObservableCollection<Zbor>(model.PoslanePrijaveZborovi);

        }
        public async Task PrihvatiPoziv(Zbor zbor)
        {
            var poziv = model.MojiPozivi.Where(p => p.IdZbor == zbor.Id).SingleOrDefault();
            await _apiServices.PrihvatiPoziv(poziv.Id);
            PoziviZborovi.Remove(zbor);
            MojiZborovi.Add(zbor);
        }
        public async Task OdbijPoziv(Zbor zbor)
        {
            var poziv = model.MojiPozivi.Where(p => p.IdZbor == zbor.Id).SingleOrDefault();
            await _apiServices.OdbijPoziv(poziv.Id);
            PoziviZborovi.Remove(zbor);
        }
        public async Task PovuciPrijavu(Zbor zbor)
        {
            await _apiServices.ObrisiPrijavuZborAsync(zbor.PrijavaZaZbor.Where(p => p.IdKorisnik == App.Korisnik.Id && p.IdZbor == zbor.Id).Single());
            PrijaveZborovi.Remove(zbor);
        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
