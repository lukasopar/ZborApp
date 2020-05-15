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
    public class JavniProfilViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;

        public event PropertyChangedEventHandler PropertyChanged;
        private string oZboru;
        public string OZboru { get { return oZboru; } set { oZboru = value; RaisePropertyChanged("OZboru"); } }
        private string repertoar;
        public string Repertoar { get { return repertoar; } set { repertoar = value; RaisePropertyChanged("Repertoar"); } }
        private string oVoditeljima;
        public string OVoditeljima { get { return oVoditeljima; } set { oVoditeljima = value; RaisePropertyChanged("OVoditeljima"); } }
        private string reprezentacija;
        public string Reprezentacija { get { return reprezentacija; } set { reprezentacija = value; RaisePropertyChanged("Reprezentacija"); } }
        private string tekst;
        public string Tekst { get { return tekst; } set { tekst = value; RaisePropertyChanged("Tekst"); } }
        private ZborDataStandard.ViewModels.ZborViewModels.JavniProfilViewModel model;
        public ZborDataStandard.ViewModels.ZborViewModels.JavniProfilViewModel Model
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
        public JavniProfilViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            OZboru = ""; OVoditeljima = ""; Reprezentacija = "";Repertoar = "";
            IsBusy = true;
            GetData(id);
           // OZboru = "<html><body><b>je</b>nije</body></html>";
            /*Repertoar = Model.Zbor.ProfilZbor.Repertoar;
            Reprezentacija = Model.Zbor.ProfilZbor.Reprezentacija;
            OVoditeljima = Model.Zbor.ProfilZbor.OVoditeljima;*/

        }
        private async void GetData(Guid id)
        {
            Model = await _apiServices.JavniProfilAsync(id);
            IsBusy = false;
            OZboru = Model.Zbor.ProfilZbor.OZboru;
            Repertoar = Model.Zbor.ProfilZbor.Repertoar;
            Reprezentacija = Model.Zbor.ProfilZbor.Reprezentacija;
            OVoditeljima = Model.Zbor.ProfilZbor.OVoditeljima;
            if (Model.Prijava) Tekst = "Prijava"; else Tekst = "Povuci prijavu";
            Clan = !(Model.Clan);
        }
        public async void SpremiOZboru(string tekst)
        {
            await _apiServices.SpremiOZboruAsync(model.Zbor.Id, tekst);
        }
        public async void SpremiOVoditeljima(string tekst)
        {
            await _apiServices.SpremiOVoditeljimauAsync(model.Zbor.Id, tekst);
        }
        public async void SpremiRepertoar(string tekst)
        {
            await _apiServices.SpremiRepertoarAsync(model.Zbor.Id, tekst);
        }
        public async void SpremiReprezentacija(string tekst)
        {
            await _apiServices.SpremiReprezentacijauAsync(model.Zbor.Id, tekst);
        }
        public async void PrijavaZbor(Guid id)
        {
            Clan = !Clan;
            Model.Prijava = !Model.Prijava;
            if (Model.Prijava) Tekst = "Prijava"; else Tekst = "Povuci prijavu";
            await _apiServices.PrijavaZborAsync(id);
        }

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
