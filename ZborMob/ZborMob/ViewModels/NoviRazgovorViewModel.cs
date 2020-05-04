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
    public class NoviRazgovorViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;

        private ObservableCollection<Korisnik> korisnici;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Korisnik> Korisnici
        {
            get
            {
                return korisnici;
            }
            set
            {
                korisnici = value;
                RaisePropertyChanged("Korisnici");

            }
        }
        private string newText;
        public string NewText
        {
            get { return newText; }
            set
            {
                newText = value;
                RaisePropertyChanged("NewText");
            }
        }
        public NoviRazgovorViewModel()
        {
            _apiServices = new ApiServices();
            GetData();

        }
        private async void GetData()
        {
            Korisnici = new ObservableCollection<Korisnik>( await _apiServices.KorisniciAsync());

        }
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
