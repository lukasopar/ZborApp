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
    public class KomentariViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<KomentarObavijesti> komentari;

        public Guid IdKorisnik { get; set; }

        public ObservableCollection<KomentarObavijesti> Komentari
        {
            get
            {
                return komentari;
            }
            set
            {
                komentari = value;
                RaisepropertyChanged("Komentari");
            }
        }
      
        private Obavijest model;

        public KomentariViewModel(Obavijest o)
        {
            _apiServices = new ApiServices();
            model = o;

        }
       
        public async void Lajk(KomentarObavijesti o)
        {
            if (!o.LajkKomentara.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id)) 
                await _apiServices.LajkKomentaraAsync(o.Id);
            else
                await _apiServices.UnLajkKomentaraAsync(o.Id);



        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
