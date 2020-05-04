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
    public class DogadjajViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public Dogadjaj Dogadjaj { get; set; }
        public DateTime DatumPocetka { get; set; }
        public DateTime DatumKraja { get; set; }
        public TimeSpan VrijemePocetka { get; set; }
        public TimeSpan VrijemeKraja { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<VrstaDogadjaja> vrste;
        public ObservableCollection<VrstaDogadjaja> Vrste
        {
            get
            {
                return vrste;
            }
            set
            {
                vrste = value;
                RaisePropertyChanged("Vrste");
            }
        }
        public Guid IdProjekt { get; set; }
        public DogadjajViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            var model = new ZborDataStandard.ViewModels.ZborViewModels.DogadjajViewModel();
            Dogadjaj = new Dogadjaj();
            IdProjekt = id;
            GetVrsta();

        }
        public DogadjajViewModel(Dogadjaj d)
        {
            _apiServices = new ApiServices();
            var model = new ZborDataStandard.ViewModels.ZborViewModels.DogadjajViewModel();
            Dogadjaj = d;
            GetVrsta();

        }
        private async void GetVrsta()
        {
            Vrste = new ObservableCollection<VrstaDogadjaja>( await _apiServices.VrsteDogadjajaAsync());
        }
        public async void OsvjeziDogadjaj(VrstaDogadjaja vrsta)
        {
            if(Dogadjaj.Id == Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                Dogadjaj.IdProjekt = IdProjekt;
            }
            var pocetak = new DateTime(DatumPocetka.Year, DatumPocetka.Month, DatumPocetka.Day, VrijemePocetka.Hours, VrijemePocetka.Minutes, VrijemePocetka.Seconds);
            var kraj = new DateTime(DatumKraja.Year, DatumKraja.Month, DatumKraja.Day, VrijemeKraja.Hours, VrijemeKraja.Minutes, VrijemeKraja.Seconds);
            Dogadjaj.DatumIvrijeme = pocetak;
            Dogadjaj.DatumIvrijemeKraja = kraj;
            Dogadjaj.IdProjekt1 = null;
            Dogadjaj.IdVrsteDogadjaja = vrsta.Id;
            await _apiServices.DogadjajAsync(IdProjekt, Dogadjaj);
        }
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
