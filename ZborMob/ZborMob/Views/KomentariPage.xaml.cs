using Syncfusion.XForms.BadgeView;
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
    public partial class KomentariPage : ContentPage
    {
        KomentariViewModel viewmodel;
        Obavijest obavijest;
        public KomentariPage(Obavijest o)
        {
            obavijest = o;
            viewmodel = new KomentariViewModel(o);
            this.BindingContext = viewmodel;

            InitializeComponent();
        }

        async void Lajkam(object sender, EventArgs args)
        {
            var k = (KomentarObavijesti)((Image)sender).BindingContext;
            await viewmodel.Lajk(k);
            if (!k.LajkKomentara.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
            {
                ((Image)sender).Source = "like.png";
                var v = (SfBadgeView)(((Image)sender).Parent);
                v.BadgeText = "" + (Int32.Parse(v.BadgeText) - 1);
            }
            else
            {

                ((Image)sender).Source = "likes.png";
                var v = (SfBadgeView)(((Image)sender).Parent);
                v.BadgeText = "" + (Int32.Parse(v.BadgeText) + 1);

            }
            int g = 0;
        }
        async void NoviKomentar(object sender, EventArgs args)
        {
            await viewmodel.DodajNovi();
            Novi.Text = "";
            listView.ScrollTo(viewmodel.Komentari[viewmodel.Komentari.Count - 1], ScrollToPosition.End, true);
            int g = 0;
        }
    }
}