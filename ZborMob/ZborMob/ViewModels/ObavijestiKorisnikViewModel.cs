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
using ZborMob.Model;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class ObavijestiKorisnikViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<OsobneObavijesti> obavijesti;
        public ObservableCollection<OsobneObavijesti> Obavijesti
        {
            get
            {
                return obavijesti;
            }
            set
            {
                obavijesti = value;
                RaisePropertyChanged("Obavijesti");
            }
        }
        
        public ObavijestiKorisnikViewModel()
        {
            _apiServices = new ApiServices();
            GetData();

        }

        async void GetData()
        {
            //model = await _apiServices.PocetnaAsync();
            var model = await _apiServices.Obavijesti();
            Obavijesti = new ObservableCollection<OsobneObavijesti>(model.Obavijesti);
        }
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
