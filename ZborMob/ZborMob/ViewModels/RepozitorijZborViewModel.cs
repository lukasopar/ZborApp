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
    public class RepozitorijZborViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<RepozitorijZbor> datoteke;
        public ObservableCollection<RepozitorijZbor> Datoteke
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
        public ZborDataStandard.ViewModels.RepozitorijViewModels.RepozitorijZborViewModel model { get; set; }
        public RepozitorijZborViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            GetData();

        }
       
        private async void GetData()
        {
            model = await _apiServices.RepozitorijZborAsync(App.Zbor.Id);
            Datoteke = new ObservableCollection<RepozitorijZbor>(model.Datoteke);
        }
        public async void Vidljivost(RepozitorijZbor dat)
        {
            await _apiServices.VidljivostZborAsync(dat.Id, dat.Privatno ? "privatizirajzbor" : "objavizbor");
        }
        public async void Obrisi(RepozitorijZbor dat)
        {           
            Datoteke.Remove(dat);

            await _apiServices.ObrisiRepozitorijZborAsync(dat.Id);
        }
        public  void Preuzmi(RepozitorijZbor dat)
        {

            _apiServices.PreuzmiZborAsync(dat);
        }

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
