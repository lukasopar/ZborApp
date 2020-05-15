using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
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
    public partial class EvidencijaPage : ContentPage
    {
        PogledajDogadjajViewModel model;
        List<Guid> guids = new List<Guid>();
        public EvidencijaPage(PogledajDogadjajViewModel model)
        {

            InitializeComponent();
               this.model = model;
            ScrollView scrollView = new ScrollView();
            StackLayout glavni = new StackLayout();
            glavni.Children.Add(new Label { Text = "Evidencija", FontSize = 20, Padding = new Thickness(10, 10) });
            foreach (var glas in model.model.Dogadjaj.IdProjektNavigation.IdVrstePodjeleNavigation.Glasovi())
            {
                SfCardView view = new SfCardView { Padding = new Thickness(10, 10) };
                Label l = new Label { Text = glas + ":",  FontSize = 17, FontAttributes = FontAttributes.Bold };
                StackLayout so = new StackLayout();
                so.Children.Add(l);
                foreach (var clan in model.model.ClanoviProjekta[glas])
                {
                    Label ime = new Label { Text = clan.IdKorisnikNavigation.ImeIPrezimeP };
                    Switch cell = new Switch();
                    if (model.model.Evidencija.Contains(clan.IdKorisnik))
                    {
                        cell.IsToggled = true;
                        guids.Add(clan.IdKorisnik);
                    }
                    cell.BindingContext = clan.IdKorisnik;
                    cell.HorizontalOptions = LayoutOptions.End;
                    cell.Toggled += Cell_Toggled;
                    Grid grid = new Grid();
                    grid.RowDefinitions.Add(new RowDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                  

                    grid.Children.Add(ime, 0, 0);
                    grid.Children.Add(cell, 1, 0);
                    so.Children.Add(grid);
                }
                view.Content = so;
                glavni.Children.Add(view);
            }
            SfCardView view1 = new SfCardView { Padding = new Thickness(10, 10) };
            Label li = new Label { Text = "Nerazvrstani", FontSize = 17, FontAttributes = FontAttributes.Bold };
            StackLayout so1 = new StackLayout();
            so1.Children.Add(li);
            foreach (var clan in model.model.NerazvrstaniClanovi)
            {
                Label ime = new Label { Text = clan.IdKorisnikNavigation.ImeIPrezimeP };
                Switch cell = new Switch();
                if (model.model.Evidencija.Contains(clan.IdKorisnik))
                {
                    cell.IsToggled = true;
                    guids.Add(clan.IdKorisnik);
                }
                cell.BindingContext = clan.IdKorisnik;
                cell.HorizontalOptions = LayoutOptions.End;
                cell.Toggled += Cell_Toggled;
                Grid grid = new Grid();

                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());


                grid.Children.Add(ime, 0, 0);
                grid.Children.Add(cell, 1, 0);
                so1.Children.Add(grid);
            }
            view1.Content = so1;
            glavni.Children.Add(view1);

            scrollView.Content = glavni;
            layout.Children.Add(scrollView);
            var btn = new Button { Text = "Spremi", BackgroundColor=Color.FromHex("1C6EBC"), TextColor=Color.White };
            btn.Clicked += Spremi;
            layout.Children.Add(btn);
            this.Content = layout;
        }

        private void Cell_Toggled(object sender, ToggledEventArgs e)
        {
            var o = (Switch)sender;
            if (e.Value)
                guids.Add((Guid)o.BindingContext);
            else
                guids.Remove((Guid)o.BindingContext);
        }

        public async void Spremi(object o, EventArgs e)
        {
            model.Spremi(guids);
            await Navigation.PopModalAsync();
        }
    }
}