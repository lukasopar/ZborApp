using Plugin.FilePicker.Abstractions;
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
    public class RepozitorijKorisnikViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<RepozitorijKorisnik> datoteke;
        public ObservableCollection<RepozitorijKorisnik> Datoteke
        {
            get
            {
                return datoteke;
            }
            set
            {
                datoteke = value;
                RaisePropertyChanged("Datoteke");
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
        public ZborDataStandard.ViewModels.RepozitorijViewModels.RepozitorijViewModel model { get; set; }
        public RepozitorijKorisnikViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            IsBusy = true;
            GetData(id);

        }
       
        private async void GetData(Guid id)
        {
            model = await _apiServices.RepozitorijKorisnikAsync(id);
            IsBusy = false;
            Clan = model.IdTrazeni == App.Korisnik.Id;
            Datoteke = new ObservableCollection<RepozitorijKorisnik>(model.Datoteke);
        }
        public async void Vidljivost(RepozitorijKorisnik dat)
        {
            await _apiServices.VidljivostKorisnikAsync(dat.Id, dat.Privatno ? "privatizirajkorisnik" : "objavikorisnik");
        }
        public async void Obrisi(RepozitorijKorisnik dat)
        {           
            Datoteke.Remove(dat);

            await _apiServices.ObrisiRepozitorijKorisnikAsync(dat.Id);
        }
        public  void Preuzmi(RepozitorijKorisnik dat)
        {

            _apiServices.PreuzmiKorisnikAsync(dat);
        }
        public async void Upload(FileData data)
        {

            var dat = await _apiServices.UploadKorisnikAsync(App.Korisnik.Id, data.FilePath, data.FileName);
            Datoteke.Insert(0, dat);
        }

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
