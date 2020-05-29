using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborMob.ViewModels;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JavniProfilKorisnikPage : TabbedPage
    {
        private JavniProfilKorisnikViewModel model;
        public JavniProfilKorisnikPage(Guid id)
        {
            model = new JavniProfilKorisnikViewModel(id);
            BindingContext = model;
            InitializeComponent();

        }
        public JavniProfilKorisnikPage()
        {
            model = new JavniProfilKorisnikViewModel(App.Korisnik.Id);
            BindingContext = model;
            InitializeComponent();
            
        }
       public void SpremiOMeni(object o, EventArgs args)
        {
            model.SpremiOMeni(oMeni.Html);
        }
       
        public async void Galerija(object o, EventArgs args)
        {
            await Navigation.PushAsync(new GalerijaKorisnik(App.Korisnik.Id));
        }
    }
}