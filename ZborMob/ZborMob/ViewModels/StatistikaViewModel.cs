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
    public class StatistikaViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Dogadjaj> evidentirani;
        public ObservableCollection<Dogadjaj> Evidentirani
        {
            get
            {
                return evidentirani;
            }
            set
            {
                evidentirani = value;
                RaisePropertyChanged("Evidentirani");
            }
        }
        public ObservableCollection<Dogadjaj> neevidentirani;

        public ObservableCollection<Dogadjaj> Neevidentirani
        {
            get
            {
                return neevidentirani;
            }
            set
            {
                neevidentirani = value;
                RaisePropertyChanged("Neevidentirani");
            }
        }
       private double postotak;
        public double Postotak
        {
            get
            {
                return postotak;
            }
            set
            {
                postotak = value;
                RaisePropertyChanged("Postotak");
            }
        }
        public Statistika model { get; set; }
        public StatistikaViewModel(Guid idKorisnik, Guid idProjekt)
        {
            _apiServices = new ApiServices();
            GetData(idKorisnik , idProjekt);

        }

        async void GetData(Guid idKorisnik, Guid idProjekt)
        {
            //model = await _apiServices.PocetnaAsync();
            model = await _apiServices.DohvatiStatistikuAsync(idKorisnik, idProjekt);
            Evidentirani = new ObservableCollection<Dogadjaj>(model.Evidentirani);
            Neevidentirani = new ObservableCollection<Dogadjaj>(model.Neevidentirani);
            Postotak = model.Postotak;
        }
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
