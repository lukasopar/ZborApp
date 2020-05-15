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
    public class ProjektiViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Projekt> mojiProjekti;
        public ObservableCollection<Projekt> MojiProjekti
        {
            get
            {
                return mojiProjekti;
            }
            set
            {
                mojiProjekti = value;
                RaisepropertyChanged("MojiProjekti");
            }
        }
        private ObservableCollection<Projekt> ostaliProjekti;
        public ObservableCollection<Projekt> OstaliProjekti
        {
            get
            {
                return ostaliProjekti;
            }
            set
            {
                ostaliProjekti = value;
                RaisepropertyChanged("OstaliProjekti");
            }
        }
        private ObservableCollection<Projekt> zavrseniProjekti;
        public ObservableCollection<Projekt> ZavrseniProjekti
        {
            get
            {
                return zavrseniProjekti;
            }
            set
            {
                zavrseniProjekti = value;
                RaisepropertyChanged("ZavrseniProjekti");
            }
        }
        private ObservableCollection<Projekt> prijavljeniProjekti;
        public ObservableCollection<Projekt> PrijavljeniProjekti
        {
            get
            {
                return prijavljeniProjekti;
            }
            set
            {
                prijavljeniProjekti = value;
                RaisepropertyChanged("PrijavljeniProjekti");
            }
        }
        public Projekt Novi { get; set; }
        private ObservableCollection<VrstaPodjele> vrste;
        public ObservableCollection<VrstaPodjele> Vrste
        {
            get
            {
                return vrste;
            }
            set
            {
                vrste = value;
                RaisepropertyChanged("Vrste");
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
                RaisepropertyChanged("IsBusy");
            }
        }
        private bool mod;
        public bool Mod
        {
            get
            {
                return mod;
            }
            set
            {
                mod = value;
                RaisepropertyChanged("Mod");
            }
        }
        public DateTime Datum { get; set; }
        public ProjektiViewModel()
        {
            _apiServices = new ApiServices();
            IsBusy = true;
            GetData();

        }
        async void GetData()
        {
            var model = await _apiServices.ProjektiAsync(App.Zbor.Id);
            IsBusy = false;
            Mod = model.Admin;
            MojiProjekti = new ObservableCollection<Projekt>(model.MojiProjekti);
            OstaliProjekti = new ObservableCollection<Projekt>(model.OstaliProjekti);
            ZavrseniProjekti = new ObservableCollection<Projekt>(model.ZavrseniProjekti);
            PrijavljeniProjekti = new ObservableCollection<Projekt>(model.PrijavaProjekti);
            Vrste = new ObservableCollection<VrstaPodjele>(model.VrstePodjele);
            Novi = new Projekt();
        }

        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public async void PrijavaProjekt(Projekt projekt)
        {
            ostaliProjekti.Remove(projekt);
            prijavljeniProjekti.Add(projekt);
            await _apiServices.PrijavaProjektAsync(projekt.Id);
        }
        public async void ObrisiPrijavu(Projekt projekt)
        {
            prijavljeniProjekti.Remove(projekt);
            ostaliProjekti.Add(projekt);

            await _apiServices.ObrisiPrijavuProjektAsync(projekt.Id);
        }
        public async void NoviProjekt(VrstaPodjele podjela)
        {
            Novi.IdVrstePodjeleNavigation = podjela;
            Novi.IdZbor = App.Zbor.Id;
            Novi.DatumPocetka = Datum;
            var pr = await _apiServices.NoviProjektAsync(Novi);
            MojiProjekti.Add(pr);
        }

    }
}
