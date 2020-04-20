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
            this.BindingContext = obavijest;
            viewmodel = new KomentariViewModel(o);
            InitializeComponent();
        }

        async void Lajkam(object sender, EventArgs args)
        {
            var k = (KomentarObavijesti)((Button)sender).BindingContext;
            viewmodel.Lajk(k);
            if (!k.LajkKomentara.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
            {
                k.LajkKomentara.Add(new LajkKomentara { Id = Guid.NewGuid(), IdKorisnik= App.Korisnik.Id, IdKomentar = k.Id });
                ((Button)sender).BackgroundColor = Xamarin.Forms.Color.Aqua;
            }
            else
            {
                var l = k.LajkKomentara.Where(l => l.IdKorisnik == App.Korisnik.Id && l.IdKomentar == k.Id).SingleOrDefault();
                k.LajkKomentara.Remove(l);
                ((Button)sender).BackgroundColor = Color.DarkCyan;

            }
            int g = 0;
        }
    }
}