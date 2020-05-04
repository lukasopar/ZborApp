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
    public partial class AdministracijaProjektaPage : ContentPage
    {
        private AdministracijaProjektaViewModel model;
        public AdministracijaProjektaPage(Guid id)
        {
            model = new AdministracijaProjektaViewModel(id);
            BindingContext = model;
            InitializeComponent();
        }
        private void PrihvatiPrijavu(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            model.PrihvatiPrijavu((PrijavaZaProjekt)btn.BindingContext);
        }
        private void ObrisiPrijavu(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            model.ObrisiPrijavu((PrijavaZaProjekt)btn.BindingContext);
        }
        private void Napunjeno(object sender, EventArgs e)
        {
            int g = 0;

            if (model.Clanovi != null && model.Napunjeno == false)
            {
                var personDataTemplate = new DataTemplate(() =>
                {
                    var lay = new StackLayout();
                    var lab = new Label();
                    var btn = new Button();
                    btn.Clicked += Statistika;
                    btn.Text = "Statistika dolazaka";
                    lab.SetBinding(Label.TextProperty, "IdKorisnikNavigation.ImeIPrezimeP");

                    lay.Children.Add(lab);
                    lay.Children.Add(btn);
                    return new ViewCell { View = lay };
                });
                foreach (var glas in model.Projekt.IdVrstePodjeleNavigation.Glasovi())
                {
                    Label l = new Label { Text = glas };
                    ListView list = new ListView
                    {
                        ItemsSource = model.Clanovi[glas],
                        ItemTemplate = personDataTemplate
                        
                    };
                    list.ItemTapped += PokreniPopup;
                    layout.Children.Add(l);
                    layout.Children.Add(list);

                }
                model.Napunjeno = true;
            }
        }
        private void Dodaj(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            model.DodajClan((Korisnik)btn.BindingContext);

        }

        private async void OdabranaPrijava(object sender, ItemTappedEventArgs e)
        {
            await DisplayAlert("Poruka kandidata", (e.Item as PrijavaZaProjekt).Poruka, "OK");
        }
    
        private void Pretrazi(object sender, EventArgs e)
        {
            model.Pretrazi();

        }
        private async void Statistika(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            var b = btn.BindingContext as ClanNaProjektu;
            try
            {
                await Navigation.PushPopupAsync(new StatistikaPopupPage(b));

            } catch (Exception exx){
                var g = exx.InnerException;
            }
        }
        private async void Zatvori(object sender, EventArgs e)
        {
            model.Zavrsi();
        }
        private async void Obrisi(object sender, EventArgs e)
        {
            model.Obrisi();
        }


        private async void PokreniPopup(object o, ItemTappedEventArgs e)
        {
            await Navigation.PushPopupAsync(new AdministracijaProjektaPopupPage(model, e.Item as ClanNaProjektu));
        }
    }
}