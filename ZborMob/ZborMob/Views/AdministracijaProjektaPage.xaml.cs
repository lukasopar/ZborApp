using Rg.Plugins.Popup.Extensions;
using Syncfusion.XForms.Cards;
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
    public partial class AdministracijaProjektaPage : TabbedPage
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
                    Grid grid = new Grid();
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition()); var lab = new Label();
                    var btn = new Button { BackgroundColor=Color.FromHex("1C6EBC"), TextColor = Color.White };
                    btn.Clicked += Statistika;
                    btn.Text = "Statistika";
                    btn.HorizontalOptions = LayoutOptions.End;
                    lab.SetBinding(Label.TextProperty, "IdKorisnikNavigation.ImeIPrezimeP");

                    grid.Children.Add(lab, 0, 0);
                    grid.Children.Add(btn, 1, 0);
                    return new ViewCell { View = grid };
                });
                ScrollView scrollView = new ScrollView();
                StackLayout glavni = new StackLayout();
                foreach (var glas in model.Projekt.IdVrstePodjeleNavigation.Glasovi())
                {
                    

                    SfCardView view = new SfCardView { Padding = new Thickness(10, 10) };
                    Label l = new Label { Text = glas, FontSize = 17, FontAttributes = FontAttributes.Bold };
                    StackLayout so = new StackLayout();
                    so.Children.Add(l);
                    /*foreach (var clan in model.Clanovi[glas])
                    {
                        Label ime = new Label { Text = clan.IdKorisnikNavigation.ImeIPrezimeP };
                        var tapGestureRecognizer = new TapGestureRecognizer();
                        tapGestureRecognizer.Tapped += PokreniPopup;
                        ime.GestureRecognizers.Add(tapGestureRecognizer);
                        ime.BindingContext = clan;

                        Button btn = new Button {Text="Statistika", BackgroundColor = Color.FromHex("1C6EBC") , TextColor=Color.White};
                        btn.BindingContext = clan;
                        btn.HorizontalOptions = LayoutOptions.End;
                        btn.Clicked += Statistika;
                        Grid grid = new Grid();
                        grid.RowDefinitions.Add(new RowDefinition());
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.ColumnDefinitions.Add(new ColumnDefinition());


                        grid.Children.Add(ime, 0, 0);
                        grid.Children.Add(btn, 1, 0);
                        so.Children.Add(grid);
                    }*/
                    ListView list = new ListView
                    {
                        ItemsSource = model.Clanovi[glas],
                        ItemTemplate = personDataTemplate,
                        HeightRequest = 200                        
                    };
                    list.ItemSelected += PokreniPopup;

                    so.Children.Add(list);
                    view.Content = so;
                    glavni.Children.Add(view);
                }

                SfCardView view1 = new SfCardView { Padding = new Thickness(10, 10) };
                Label li = new Label { Text = "Nerazvrstani", FontSize = 17, FontAttributes = FontAttributes.Bold };
                StackLayout so1 = new StackLayout();
                so1.Children.Add(li);
                /* foreach (var clan in model.Nerazvrstani)
                 {
                     Label ime = new Label { Text = clan.IdKorisnikNavigation.ImeIPrezimeP };
                     var tapGestureRecognizer = new TapGestureRecognizer();
                    // tapGestureRecognizer.Tapped += PokreniPopup;
                     ime.GestureRecognizers.Add(tapGestureRecognizer);
                     ime.BindingContext = clan;

                     Button btn = new Button { Text = "Statistika", BackgroundColor = Color.FromHex("1C6EBC"), TextColor = Color.White };
                     btn.BindingContext = clan;
                     btn.HorizontalOptions = LayoutOptions.End;
                     btn.Clicked += Statistika;
                     Grid grid = new Grid();
                     grid.RowDefinitions.Add(new RowDefinition());
                     grid.ColumnDefinitions.Add(new ColumnDefinition());
                     grid.ColumnDefinitions.Add(new ColumnDefinition());


                     grid.Children.Add(ime, 0, 0);
                     grid.Children.Add(btn, 1, 0);
                     so1.Children.Add(grid);
                 }*/
                ListView list1 = new ListView
                {
                    ItemsSource = model.Nerazvrstani,
                    ItemTemplate = personDataTemplate,
                    HeightRequest = 200
                };
                list1.ItemSelected += PokreniPopup;

                so1.Children.Add(list1);
                view1.Content = so1;
                glavni.Children.Add(view1);
               


                model.Napunjeno = true;
                scrollView.Content = glavni;
                Clanovi.Content = scrollView;
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
                await Navigation.PushModalAsync(new StatistikaPopupPage(b));

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


        private async void PokreniPopup(object o, SelectedItemChangedEventArgs e)
        {
            await Navigation.PushPopupAsync(new AdministracijaProjektaPopupPage(model, e.SelectedItem as ClanNaProjektu));
        }
    }
}