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
    public partial class NajavePopupPage : ContentPage
    {
        PogledajDogadjajViewModel model;
        public NajavePopupPage(PogledajDogadjajViewModel model)
        {

            InitializeComponent();
               this.model = model;
          
            ScrollView scrollView = new ScrollView();
            StackLayout glavni = new StackLayout();
            glavni.Children.Add(new Label { Text = "Najave", FontSize = 20 , Padding = new Thickness(10,10)});
            foreach (var glas in model.model.Dogadjaj.IdProjektNavigation.IdVrstePodjeleNavigation.Glasovi())
            {
                SfCardView view = new SfCardView {Padding = new Thickness(10, 10) };
                Label l = new Label { Text = glas + ": " + model.model.Clanovi[glas].Count, FontSize=17, FontAttributes=FontAttributes.Bold };
                StackLayout so = new StackLayout();
                so.Children.Add(l);
                foreach(var najava in model.model.Clanovi[glas])
                {
                    var lab = new Label { Text = najava.IdKorisnikNavigation.ImeIPrezimeP };
                    so.Children.Add(lab);
                }
                view.Content = so;
                glavni.Children.Add(view);

            }

            SfCardView view1 = new SfCardView { Padding = new Thickness(10, 10) };
            Label li = new Label { Text = "Nerazvrstani" + ": " + model.model.Nerazvrstani.Count, FontSize = 17, FontAttributes = FontAttributes.Bold };
            StackLayout so1 = new StackLayout();
            so1.Children.Add(li);
            foreach (var najava in model.model.Nerazvrstani)
            {
                var lab = new Label { Text = najava.IdKorisnikNavigation.ImeIPrezimeP };
                so1.Children.Add(lab);
            }
            view1.Content = so1;
            glavni.Children.Add(view1);

            scrollView.Content = glavni;
            this.Content = scrollView;


        }

    }
}