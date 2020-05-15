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
    public partial class ProjektiPage : TabbedPage
    {
        private ProjektiViewModel model;
        public ProjektiPage()
        {
            model = new ProjektiViewModel();
            BindingContext = model;
            InitializeComponent();
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
      
        private async void Administracija(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            await Navigation.PushAsync(new AdministracijaProjektaPage(((Projekt)btn.BindingContext).Id));
        }
        private async void Novi(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new NoviProjekt(model));
        }
        private async void OtvoriProjekt(object o, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new ProjektTabbedPage(new ProjektViewModel(((Projekt)e.ItemData).Id)));
        }
    }
}