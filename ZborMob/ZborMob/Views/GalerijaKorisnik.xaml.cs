using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborDataStandard.Model;
using ZborMob.ViewModels;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GalerijaKorisnik : ContentPage
    {
        GalerijaKorisnikViewModel model;
        public GalerijaKorisnik(Guid id)
        {
            model = new GalerijaKorisnikViewModel(id);
            BindingContext = model;
            InitializeComponent();
        }
        public async void Profilna(object o, EventArgs e)
        {
            var btn = (Button)o;
            model.Profilna(((RepozitorijKorisnik)btn.BindingContext).Id);
            App.Korisnik.IdSlika = ((RepozitorijKorisnik)btn.BindingContext).Id;
        }
    }
}