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
            var k = (KomentarObavijesti)((Button)sender).BindingContext;
            viewmodel.Lajk(k);
            if (!k.LajkKomentara.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
            {
                ((Button)sender).BackgroundColor = Xamarin.Forms.Color.Aqua;
            }
            else
            {
                
                ((Button)sender).BackgroundColor = Color.DarkCyan;

            }
            int g = 0;
        }
        async void NoviKomentar(object sender, EventArgs args)
        {
            viewmodel.DodajNovi();
            int g = 0;
        }
    }
}