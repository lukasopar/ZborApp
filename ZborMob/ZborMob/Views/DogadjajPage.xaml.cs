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
    public partial class DogadjajPage : ContentPage
    {
        private Dogadjaj dog;
        private PogledajDogadjajViewModel model;
        public DogadjajPage(Dogadjaj d)
        {
            dog = d;
            BindingContext = dog;
            model = new PogledajDogadjajViewModel(dog.Id);
            InitializeComponent();
        }
        private async void Najave(object o, EventArgs e)
        {
            await Navigation.PushModalAsync(new NajavePopupPage(model));
        }
        private async void Evidencija(object o, EventArgs e)
        {
            await Navigation.PushModalAsync(new EvidencijaPage(model));
        }
    }
}