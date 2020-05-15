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
    public partial class ProjektTabbedPage : TabbedPage
    {
        ProjektViewModel viewmodel;
        public ProjektTabbedPage(ProjektViewModel model)
        {
            BindingContext = model;
            viewmodel = model;
            InitializeComponent();
        }
        async void Lajkam(object sender, EventArgs args)
        {
            var obavijest = (Obavijest)((Image)sender).BindingContext;
            viewmodel.Lajk(obavijest);
            if (!obavijest.LajkObavijesti.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
            {
                obavijest.LajkObavijesti.Add(new LajkObavijesti { Id = Guid.NewGuid(), IdKorisnik = App.Korisnik.Id, IdObavijest = obavijest.Id });
                ((Image)sender).Source = "likes.png";
                var v = (SfBadgeView)(((Image)sender).Parent);
                v.BadgeText = "" + (Int32.Parse(v.BadgeText) + 1);
            }
            else
            {
                var l = obavijest.LajkObavijesti.Where(l => l.IdKorisnik == App.Korisnik.Id && l.IdObavijest == obavijest.Id).SingleOrDefault();
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
        private void Novi(object o, EventArgs e)
        {
            Navigation.PushModalAsync(new NoviDogadjajPage(viewmodel.Id));
        }
        private void ObrisiDogadjaj(object o, EventArgs e)
        {
            var dog = (Dogadjaj)((Button)o).BindingContext;
            viewmodel.ObrisiDogadjaj(dog);
        }
        async void NajaviDolazak(object sender, EventArgs args)
        {
            var dog = (Dogadjaj)((Button)sender).BindingContext;
            viewmodel.Najavi(dog);
            if (!dog.NajavaDolaska.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
            {
                dog.NajavaDolaska.Add(new NajavaDolaska { Id = Guid.NewGuid(), IdKorisnik = App.Korisnik.Id, IdDogadjaj = dog.Id });
                ((Button)sender).BackgroundColor = Color.FromHex("1C6EBC");
            }
            else
            {
                var l = dog.NajavaDolaska.Where(l => l.IdKorisnik == App.Korisnik.Id && l.IdDogadjaj == dog.Id).SingleOrDefault();
                dog.NajavaDolaska.Remove(l);
                ((Button)sender).BackgroundColor = Color.Gray;

            }
            int g = 0;
        }
        async void UrediDogadjaj(object sender, EventArgs args)
        {
            var dog = (Dogadjaj)((Button)sender).BindingContext;
            await Navigation.PushAsync(new NoviDogadjajPage(dog));
        }
        private async void Dogadjaj(object o, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new DogadjajPage((Dogadjaj)e.Item));
        }
    }
}