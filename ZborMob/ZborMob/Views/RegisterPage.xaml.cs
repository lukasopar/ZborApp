using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborMob.ViewModels;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        private RegistracijaViewModel model;
        public RegisterPage()
        {
            model = new RegistracijaViewModel();
            BindingContext = model;
            InitializeComponent();
            datum.MaximumDate = DateTime.Now;
        }
        public void Prijava(object o, EventArgs e)
        {
            App.Current.MainPage = new LoginPage();
        }
        public async void Reg(object o, EventArgs e)
        {
            reg.IsVisible = false;
            prijava.IsVisible = false;
            activity.IsVisible = true;
            var odg = await model.Reg();
            reg.IsVisible = true;
            prijava.IsVisible = true;
            activity.IsVisible = false;
            if (odg != "")
                await DisplayAlert("Greška", odg, "Ok");

        }
    }
}