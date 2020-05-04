using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
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
        public EvidencijaPage(PogledajDogadjajViewModel model)
        {

            InitializeComponent();
               this.model = model;
            var personDataTemplate = new DataTemplate(() =>
            {
                var lay = new StackLayout();
                var lab = new Label();


                lab.SetBinding(Label.TextProperty, "IdKorisnikNavigation.ImeIPrezimeP");

                lay.Children.Add(lab);

                return new ViewCell { View = lay };
            });
            foreach (var glas in model.model.Dogadjaj.IdProjektNavigation.IdVrstePodjeleNavigation.Glasovi())
            {
                Label l = new Label { Text = glas + ": " + model.model.Clanovi[glas].Count };
                layout.Children.Add(l);
                foreach(var clan in model.model.ClanoviProjekta[glas])
                {
                    Label ime = new Label { Text = clan.IdKorisnikNavigation.ImeIPrezimeP };
                    Switch cell = new Switch();
                    if (model.model.Evidencija.Contains(clan.IdKorisnik))
                        cell.IsToggled = true;
                    cell.BindingContext = clan.IdKorisnik;
                    StackLayout stack = new StackLayout { Orientation = StackOrientation.Horizontal };

                    stack.Children.Add(ime);
                    stack.Children.Add(cell);
                    layout.Children.Add(stack);
                }
            
            }
            Label li = new Label { Text = "Nerazvrstani" + ": " + model.model.Nerazvrstani.Count };
            layout.Children.Add(li);

            foreach (var clan in model.model.NerazvrstaniClanovi)
            {
                Label ime = new Label { Text = clan.IdKorisnikNavigation.ImeIPrezimeP };
                Switch cell = new Switch();
                if (model.model.Evidencija.Contains(clan.IdKorisnik))
                    cell.IsToggled = true;
                cell.BindingContext = clan.IdKorisnik;
                StackLayout stack = new StackLayout { Orientation = StackOrientation.Horizontal };

                stack.Children.Add(ime);
                stack.Children.Add(cell);
                layout.Children.Add(stack);
            }
        }
        public void Spremi(object o, EventArgs e)
        {

        }
    }
}