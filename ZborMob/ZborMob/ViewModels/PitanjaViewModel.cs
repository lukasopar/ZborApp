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
    public class PitanjaViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Anketa> ankete;

        public Dictionary<Guid, List<int>> KorisnickiOdgovori;

        public ObservableCollection<Anketa> Ankete
        {
            get
            {
                return ankete;
            }
            set
            {
                ankete = value;
                RaisepropertyChanged("Ankete");
            }
        }
      
        private ZborDataStandard.ViewModels.ZborViewModels.PitanjaViewModel model;

        public PitanjaViewModel()
        {
            _apiServices = new ApiServices();
            GetData();

        }
        async void GetData()
        {
            model = await _apiServices.PitanjaAsync(App.Zbor.Id);
            Ankete = new ObservableCollection<Anketa>(model.AktivnaPitanja);
            KorisnickiOdgovori = model.KorisnickiOdgovori;
           

        }
        public async void OdgovoriNaPitanje(Guid id, List<int> odgovori)
        {
            await _apiServices.OdgovoriNaPitanje(id, odgovori);

        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
