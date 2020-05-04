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
    public class ProjektViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Obavijest> obavijesti;
        private ObservableCollection<Dogadjaj> dogadjaji;
        public Guid Id { get; set; }

        public ObservableCollection<Obavijest> Obavijesti
        {
            get
            {
                return obavijesti;
            }
            set
            {
                obavijesti = value;
                RaisepropertyChanged("Obavijesti");
            }
        }
        public ObservableCollection<Dogadjaj> Dogadjaji
        {
            get
            {
                return dogadjaji;
            }
            set
            {
                dogadjaji = value;
                RaisepropertyChanged("Dogadjaji");
            }
        }

        private ZborDataStandard.ViewModels.ZborViewModels.ProjektViewModel model;

        public ProjektViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            Id = id;
            GetData(id);

        }
        async void GetData(Guid id)
        {
            model = await _apiServices.ProjektAsync(id);
            Obavijesti = new ObservableCollection<Obavijest>(model.Obavijesti);
            //IdKorisnik = model.IdKorisnik;
            Dogadjaji = new ObservableCollection<Dogadjaj>(model.AktivniDogadjaji);
        }
        public async void Lajk(Obavijest o)
        {
            if (!o.LajkObavijesti.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id)) 
                await _apiServices.LajkObavijestiAsync(o.Id);
            else
                await _apiServices.UnLajkObavijestiAsync(o.Id);



        }
        public async void Najavi(Dogadjaj o)
        {
            if (!o.NajavaDolaska.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
                await _apiServices.NajavaDolaska(o.Id);
            else
                await _apiServices.UnNajavaDolaska(o.Id);
        }
        public async void ObrisiDogadjaj(Dogadjaj o)
        {
            await _apiServices.ObrisiDogadjaj(o.Id);
            Dogadjaji.Remove(o);
        }

        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
