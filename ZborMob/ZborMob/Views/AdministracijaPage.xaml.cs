using Rg.Plugins.Popup.Extensions;
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
    public partial class AdministracijaPage : TabbedPage
    {
        private AdministracijaViewModel model;
        public AdministracijaPage()
        {
            model = new AdministracijaViewModel();
            BindingContext = model;
            InitializeComponent();
        }
        private void PrihvatiPrijavu(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            model.PrihvatiPrijavu((PrijavaZaZbor)btn.BindingContext);
        }
        private void ObrisiPrijavu(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            model.ObrisiPrijavu((PrijavaZaZbor)btn.BindingContext);
        }
        private void ObrisiPoziv(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            model.ObrisiPoziv((PozivZaZbor)btn.BindingContext);
        }
        private async void OdabranaPrijava(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs  e)
        {
            await DisplayAlert("Poruka kandidata", (e.ItemData as PrijavaZaZbor).Poruka, "OK");
        }
        private async void OdabraniPoziv(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs  e)
        {
            await DisplayAlert("Poruka voditelja", (e.ItemData as PozivZaZbor).Poruka, "OK");
        }
        private void Pretrazi(object sender, EventArgs e)
        {
            model.Pretrazi();

        }
        private async void ObrisiModeratora(object o, EventArgs e)
        {
            Button btn = (Button)o;
            model.ObrisiModeratora((ModeratorZbora)btn.BindingContext);
        }
        private void Pozovi(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            model.Pozovi((Korisnik)btn.BindingContext);
        }
        private async void PokreniPopup(object o, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            await Navigation.PushPopupAsync(new AdministracijaPopupPage(model, e.ItemData as ClanZbora));
        }
    }
}