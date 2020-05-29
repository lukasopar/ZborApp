using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class PocetnaPage : TabbedPage
    {
        PocetnaViewModel model;
        public PocetnaPage()
        {
            model = new PocetnaViewModel();
            BindingContext = model;
            InitializeComponent();
           
        }
        async void MojZborTapped(object sender, SelectedItemChangedEventArgs e)
        {
            ListView btn = (ListView)sender;
            Zbor zbor = (Zbor)btn.SelectedItem;
            App.Zbor = zbor;
            await Navigation.PushAsync(new ZborMasterPage());
        }
        async void NeMojZbor(object sender, SelectedItemChangedEventArgs e)
        {
            ListView btn = (ListView)sender;
            Zbor zbor = (Zbor)btn.SelectedItem;
            await Navigation.PushAsync(new JavniProfilPage(zbor.Id));
        }
        public void Dodaj(object o, EventArgs e)
        {
            Navigation.PushAsync(new DodajZborPage(BindingContext as PocetnaViewModel));
        }
        public  async void ObrisiPrijavu(object o, EventArgs e)
        {
            var btn = (Button)o;
            await model.PovuciPrijavu(btn.BindingContext as Zbor);
        }
        public async void PrihvatiPoziv(object o, EventArgs e)
        {
            var btn = (Button)o;
            await model.PrihvatiPoziv(btn.BindingContext as Zbor);
        }
        public async void OdbijPoziv(object o, EventArgs e)
        {
            var btn = (Button)o;
            await model.OdbijPoziv(btn.BindingContext as Zbor);
        }
    }
}