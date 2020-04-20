using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class ObavijestiPage : ContentPage
    {
        ObavijestiViewModel viewmodel;

        public ObavijestiPage(Guid id)
        {
            
            viewmodel = new ObavijestiViewModel(id);
            this.BindingContext = viewmodel;
            InitializeComponent();
        }
        async void Lajkam(object sender, EventArgs args)
        {
            var obavijest = (Obavijest)((Button)sender).BindingContext;
            viewmodel.Lajk(obavijest);
            if(!obavijest.LajkObavijesti.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
            {
                obavijest.LajkObavijesti.Add(new LajkObavijesti { Id = Guid.NewGuid(), IdKorisnik = viewmodel.IdKorisnik, IdObavijest = obavijest.Id });
                ((Button)sender).BackgroundColor = Xamarin.Forms.Color.Aqua;
            }
            else
            {
                 var l = obavijest.LajkObavijesti.Where(l => l.IdKorisnik == viewmodel.IdKorisnik && l.IdObavijest == obavijest.Id).SingleOrDefault();
                obavijest.LajkObavijesti.Remove(l);
                ((Button)sender).BackgroundColor = Color.DarkCyan;

            }
            int g = 0;
        }
        private void Komentari(object o, EventArgs e)
        {
            var obavijest = (Obavijest)((Label)o).BindingContext;
            Navigation.PushModalAsync(new KomentariPage(obavijest));
        }
    }
}