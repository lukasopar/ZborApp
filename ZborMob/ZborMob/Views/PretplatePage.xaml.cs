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
    public partial class PretplatePage : ContentPage
    {
        PretplateViewModel model;
        public PretplatePage()
        {
            model = new PretplateViewModel();
            BindingContext = model;
            InitializeComponent();
        }
        async void Spremi(object o, EventArgs e)
        {
            model.Spremi();
            await DisplayAlert("Spremljeno", "Promjene su spremljenje", "Ok");
        }
    }
}