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
    public partial class ProjektiPage : ContentPage
    {
        private ProjektiViewModel model;
        public ProjektiPage()
        {
            model = new ProjektiViewModel();
            BindingContext = model;
            InitializeComponent();
            datePicker.MinimumDate = DateTime.Now;
        }
        private void PrijavaProjekt(object o, EventArgs e)
        {
            Button btn = (Button)o;
            model.PrijavaProjekt((Projekt)btn.BindingContext);
        }
        private void ObrisiPrijavu(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            model.ObrisiPrijavu((Projekt)btn.BindingContext);
        }
        private void NoviProjekt(object sender, EventArgs e)
        {
            var vrsta = (VrstaPodjele)picker.SelectedItem;
            model.NoviProjekt(vrsta);
        }
        private async void OtvoriProjekt(object o, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new ProjektTabbedPage(new ProjektViewModel(((Projekt)e.Item).Id)));
        }
    }
}