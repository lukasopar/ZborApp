using ImageCircle.Forms.Plugin.Abstractions;
using Syncfusion.XForms.Buttons;
using Syncfusion.XForms.Cards;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using ZborDataStandard.Model;
using ZborMob.Converters;
using Switch = Xamarin.Forms.Switch;

namespace ZborMob.Views
{
    public class CustomPitanjeCell : ViewCell
    {
        public static readonly BindableProperty PitanjeProperty =  BindableProperty.Create("Pitanje", typeof(Anketa), typeof(CustomPitanjeCell));

        public Dictionary<Guid, List<int>> KorisnickiOdgovori;
        public event EventHandler<EventArgs> PromjenaOdgovora;
        public event EventHandler<EventArgs> RezultatiClick;

        public Anketa Pitanje
        {
            get { return (Anketa)GetValue(PitanjeProperty); }
            set { SetValue(PitanjeProperty, value); }
        }
        public CircleImage profilna;
        public Label labelPitanje = new Label();
        public Label autor = new Label();
        public Label datum = new Label();

        public Picker picker = new Picker();
        public StackLayout switchers = new StackLayout();
        public Button RezultatiBtn { get; set; } = new Button();

        public Grid grid;
        public CustomPitanjeCell()
        {
            var pitanje = (Anketa)GetValue(PitanjeProperty);
            //instantiate each of our views
            var card = new SfCardView
            {
                CornerRadius = 4,
                HasShadow = true,
                WidthRequest = 343,
                BackgroundColor = Color.WhiteSmoke,
                HorizontalOptions = LayoutOptions.Center,
                Padding = new Thickness(16, 16, 8, 16)
            };
            grid = new Grid
            {
                ColumnSpacing = 8,
                RowSpacing = 8,
            };
            //4 reda
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            //3 stupca
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            //profilna slika
            profilna = new CircleImage
            {
                HeightRequest = 40,
                WidthRequest = 40,
                HorizontalOptions = LayoutOptions.Start,
                BorderColor = Color.Black
                //Source = (ImageSource)converter.Convert(pitanje.IdKorisnikNavigation.IdSlika, null, null, null)
            };
            grid.Children.Add(profilna, 0, 0);
            //ime autora
            autor = new Label
            {
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.Gray,
            };
            grid.Children.Add(autor, 1, 0);
            //gumb za brisanje
            Button btn = new Button
            {
                FontFamily = "zboris.ttf#zboris",
                Text = "\ue709",
                BackgroundColor = Color.FromHex("1C6EBC"),
                TextColor = Color.White,
                WidthRequest = 40,
                HorizontalOptions = LayoutOptions.End
            };
            grid.Children.Add(btn, 2, 0);
            // pitanje
            labelPitanje.FontSize = 20;
            labelPitanje.FontAttributes = FontAttributes.Bold;
            grid.Children.Add(labelPitanje, 0, 1);
            Grid.SetColumnSpan(labelPitanje, 3);
            //odgovori
            grid.Children.Add(picker,0, 2);
            Grid.SetColumnSpan(picker, 3);
            grid.Children.Add(switchers, 0, 2);
            Grid.SetColumnSpan(switchers, 3);

            //Datum zatvaranja
            datum = new Label();
            datum.HorizontalOptions = LayoutOptions.Start;
            datum.VerticalOptions = LayoutOptions.Center;
            grid.Children.Add(datum, 0, 3);
            Grid.SetColumnSpan(datum, 2);
            
            // rezultati
            RezultatiBtn.Text = "Rezultati";
            RezultatiBtn.Clicked += (sender, args) => this.RezultatiClick.Invoke(sender, args);
            RezultatiBtn.TextColor = Color.White;
            RezultatiBtn.HeightRequest = 40;
            RezultatiBtn.BackgroundColor = Color.FromHex("1C6EBC");
            RezultatiBtn.HorizontalOptions = LayoutOptions.End;
            grid.Children.Add(RezultatiBtn, 2, 3);

            card.Content = grid;
            View = card;
        }

        
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            var pitanje = (Anketa)GetValue(PitanjeProperty);
            labelPitanje.Text = pitanje.Pitanje;
            autor.Text = pitanje.IdKorisnikNavigation.ImeIPrezimeP;
            datum.Text = "Datum zatvaranja: " + pitanje.DatumKraja.ToString("dd.MM.yyyy.");
            var converter = new SlikaLinkConverter();
            profilna.Source = (ImageSource)converter.Convert(pitanje.IdKorisnikNavigation.IdSlika, null, null, null);
            RezultatiBtn.BindingContext = pitanje;

            if (pitanje.VisestrukiOdgovor == false)
            {
                picker.ItemsSource = pitanje.OdgovorAnkete.ToList();
                picker.ItemDisplayBinding = new Binding("Odgovor");
                var odg = pitanje.OdgovorAnkete.Where(o => o.OdgovorKorisnikaNaAnketu.Select(od => od.IdKorisnik).Contains(App.Korisnik.Id)).FirstOrDefault();
                if (odg != null)
                    picker.SelectedItem = odg;
                picker.SelectedIndexChanged +=  (sender, args) => this.PromjenaOdgovora.Invoke(this, args);
                if (pitanje.DatumKraja < DateTime.Now)
                    picker.IsEnabled = false;
            }
            else
            {
                grid.Children.Remove(picker);
                foreach(var odg in pitanje.OdgovorAnkete)
                {
                    StackLayout horizontalLayout = new StackLayout();
                    horizontalLayout.Orientation = StackOrientation.Horizontal;
                    Label l = new Label { Text = odg.Odgovor };
                    Switch cell = new Switch();
                    cell.HorizontalOptions = LayoutOptions.End;
                    cell.ThumbColor = Color.AliceBlue;
                    if (odg.OdgovorKorisnikaNaAnketu.Select(o => o.IdKorisnik).Contains(App.Korisnik.Id))
                        cell.IsToggled = true;
                    horizontalLayout.Children.Add(l);
                    horizontalLayout.Children.Add(cell);
                    cell.BindingContext = odg;
                    cell.Toggled += (sender, args) => this.PromjenaOdgovora.Invoke(this, args);
                    switchers.Children.Add(horizontalLayout);
                    if (pitanje.DatumKraja < DateTime.Now)
                        cell.IsEnabled = false;
                }
            }


        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
