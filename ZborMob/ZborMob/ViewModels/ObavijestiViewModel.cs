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
    public class ObavijestiViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Obavijest> obavijesti;

        public Guid IdKorisnik { get; set; }
        public string Naslov { get; set; }
        public string Tekst { get; set; }
        private Guid IdZbor { get; set; }
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
      
        private ProfilViewModel model;

        public ObavijestiViewModel(Guid id)
        {
            _apiServices = new ApiServices();
            IdZbor = id;
            GetData(id);

        }
        async void GetData(Guid id)
        {
            model = await _apiServices.ProfilAsync(id);
            Obavijesti = new ObservableCollection<Obavijest>(model.Obavijesti);
            IdKorisnik = model.IdKorisnik;

        }
        public async void Lajk(Obavijest o)
        {
            if (!o.LajkObavijesti.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id)) 
                await _apiServices.LajkObavijestiAsync(o.Id);
            else
                await _apiServices.UnLajkObavijestiAsync(o.Id);

        }
        public async void NovaObavijest()
        {
            var o = await _apiServices.NovaObavijest(Naslov, Tekst, IdZbor);
            Obavijesti.Prepend(o);



        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
