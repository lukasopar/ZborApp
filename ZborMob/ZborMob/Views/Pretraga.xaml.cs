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
    public partial class Pretraga : ContentPage
    {
        PretragaViewModel model;
        public Pretraga()
        {
            model = new PretragaViewModel();
            BindingContext = model;
            InitializeComponent();
        }
        public void PretragaUpit(object o, EventArgs e)
        {
            model.Pretraga();
        }
        public void Zbor(object o, SelectedItemChangedEventArgs e)
        {
            var zbor = e.SelectedItem as Zbor;
            Navigation.PushAsync(new JavniProfilPage(zbor.Id));
        }
        public void Korisnik(object o, SelectedItemChangedEventArgs e)
        {
            var k = e.SelectedItem as Korisnik;
            Navigation.PushAsync(new JavniProfilKorisnikPage(k.Id));
        }
    }
}