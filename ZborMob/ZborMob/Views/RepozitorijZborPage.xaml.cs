using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborDataStandard.Model;
using ZborMob.ViewModels;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RepozitorijZborPage : ContentPage
    {
        RepozitorijZborViewModel model;
        public RepozitorijZborPage()
        {
            model = new RepozitorijZborViewModel(App.Zbor.Id);
            BindingContext = model;
            InitializeComponent();
        }

        private void Vidljivost(object sender, ToggledEventArgs e)
        {
            var src = (Switch)sender;
            model.Vidljivost((RepozitorijZbor)src.BindingContext);
        }
        private void Obrisi(object sender, EventArgs e)
        {
            var src = (Button)sender;
            model.Obrisi((RepozitorijZbor)src.BindingContext);
        }
        private void Preuzmi(object sender, EventArgs e)
        {
            var src = (Label)sender;
            model.Preuzmi((RepozitorijZbor)src.BindingContext);
        }
        private async void Podijeli(object sender, EventArgs e)
        {
            var src = (Button)sender;
            await Clipboard.SetTextAsync("https://localhost:5001/Repozitorij/GetZbor" + ((RepozitorijZbor)src.BindingContext).Url);
            await DisplayAlert("Kopirano u međuspremnik","Kopirano", "OK");

        }
    }
}