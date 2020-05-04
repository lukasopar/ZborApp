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
    public partial class NajavePopupPage : ContentPage
    {
        PogledajDogadjajViewModel model;
        public NajavePopupPage(PogledajDogadjajViewModel model)
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
                
                ListView list = new ListView
                {
                    ItemsSource = model.model.Clanovi[glas],
                    ItemTemplate = personDataTemplate
                };

                layout.Children.Add(l);
                layout.Children.Add(list);

             

            }
            Label li = new Label { Text = "Nerazvrstani" + ": " + model.model.Nerazvrstani.Count };
            
            ListView listi = new ListView
            {
                ItemsSource = model.model.Nerazvrstani,
                ItemTemplate = personDataTemplate
            };
            layout.Children.Add(li);
            layout.Children.Add(listi);


        }

    }
}