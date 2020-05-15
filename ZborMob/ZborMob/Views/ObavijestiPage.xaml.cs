using Rg.Plugins.Popup.Services;
using Syncfusion.XForms.BadgeView;
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

        public ObavijestiPage()
        {
            
            viewmodel = new ObavijestiViewModel(App.Zbor.Id);
            this.BindingContext = viewmodel;
            InitializeComponent();
        }
        async void Lajkam(object sender, EventArgs args)
        {
            var obavijest = (Obavijest)((Image)sender).BindingContext;
            viewmodel.Lajk(obavijest);
            if(!obavijest.LajkObavijesti.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
            {
                obavijest.LajkObavijesti.Add(new LajkObavijesti { Id = Guid.NewGuid(), IdKorisnik = viewmodel.IdKorisnik, IdObavijest = obavijest.Id });
                ((Image)sender).Source = "likes.png";
                var v = (SfBadgeView)(((Image)sender).Parent);
                v.BadgeText = "" + (Int32.Parse(v.BadgeText) + 1);
            }
            else
            {
                 var l = obavijest.LajkObavijesti.Where(l => l.IdKorisnik == viewmodel.IdKorisnik && l.IdObavijest == obavijest.Id).SingleOrDefault();
                obavijest.LajkObavijesti.Remove(l);
                ((Image)sender).Source = "like.png";
                var v = (SfBadgeView)(((Image)sender).Parent);
                v.BadgeText = "" + (Int32.Parse(v.BadgeText) - 1);
            }
            int g = 0;
        }
        private void Komentari(object o, EventArgs e)
        {
            var obavijest = (Obavijest)((Label)o).BindingContext;
            Navigation.PushModalAsync(new KomentariPage(obavijest));
        }
        async void NovaObavijest(object sender, EventArgs args)
        {
            viewmodel.NovaObavijest();
            int g = 0;
        }
    }
}