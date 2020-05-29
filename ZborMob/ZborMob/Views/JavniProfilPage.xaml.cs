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
    public partial class JavniProfilPage : TabbedPage
    {
        private JavniProfilViewModel model;
        public JavniProfilPage()
        {
            model = new JavniProfilViewModel(App.Zbor.Id);
            BindingContext = model;
            InitializeComponent();

        }
        public JavniProfilPage(Guid id)
        {
            model = new JavniProfilViewModel(id);
            BindingContext = model;
            InitializeComponent();
            
        }
       public void SpremiOZboru(object o, EventArgs args)
        {
            model.SpremiOZboru(oZboru.Html);
        }
        public void SpremiOVoditeljima(object o, EventArgs args)
        {
            model.SpremiOVoditeljima(oVoditeljima.Html);
        }
        public void SpremiRepertoar(object o, EventArgs args)
        {
            model.SpremiRepertoar(repertoar.Html);
        }
        public void SpremiReprezentacija(object o, EventArgs args)
        {
            model.SpremiReprezentacija(reprezentacija.Html);
        }
        public async void Galerija(object o, EventArgs args)
        {
            await Navigation.PushAsync(new GalerijaZbor(App.Zbor.Id));
        }
        public async void Repozitorij(object o, EventArgs args)
        {
            await Navigation.PushAsync(new RepozitorijZborPage());
        }
        public void Prijava(object o, EventArgs args)
        {
            model.PrijavaZbor(App.Zbor.Id);
        }
    }
}