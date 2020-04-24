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
            var obavijest = (Obavijest)((Button)sender).BindingContext;
            viewmodel.Lajk(obavijest);
            if (!obavijest.LajkObavijesti.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
            {
                obavijest.LajkObavijesti.Add(new LajkObavijesti { Id = Guid.NewGuid(), IdKorisnik = App.Korisnik.Id, IdObavijest = obavijest.Id });
                ((Button)sender).BackgroundColor = Xamarin.Forms.Color.Aqua;
            }
            else
            {
                var l = obavijest.LajkObavijesti.Where(l => l.IdKorisnik == App.Korisnik.Id && l.IdObavijest == obavijest.Id).SingleOrDefault();
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
                ((Button)sender).BackgroundColor = Xamarin.Forms.Color.Aqua;
            }
            else
            {
                var l = dog.NajavaDolaska.Where(l => l.IdKorisnik == App.Korisnik.Id && l.IdDogadjaj == dog.Id).SingleOrDefault();
                dog.NajavaDolaska.Remove(l);
                ((Button)sender).BackgroundColor = Color.DarkCyan;

            }
            int g = 0;
        }
    }
}