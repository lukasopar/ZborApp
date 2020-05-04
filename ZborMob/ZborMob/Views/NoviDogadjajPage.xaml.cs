using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborMob.ViewModels;
using ZborDataStandard.Model;
namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NoviDogadjajPage : ContentPage
    {
        private DogadjajViewModel model;
        public NoviDogadjajPage(Guid id)
        {
            model = new DogadjajViewModel(id);
            this.BindingContext = model;
            InitializeComponent();
            datumKraja.MinimumDate = DateTime.Now;
            datumPocetka.MinimumDate = DateTime.Now;
        }
        public NoviDogadjajPage(Dogadjaj d)
        {
            model = new DogadjajViewModel(d);
            this.BindingContext = model;
            InitializeComponent();
            datumKraja.Date = d.DatumIvrijemeKraja;
            datumPocetka.Date = d.DatumIvrijeme;
            vrijemeKraja.Time = d.DatumIvrijemeKraja.TimeOfDay;
            vrijemePocetka.Time = d.DatumIvrijeme.TimeOfDay;

            datumKraja.MinimumDate = DateTime.Now;
            datumPocetka.MinimumDate = DateTime.Now;
        }

        private void PromjenaPocetka(object sender, DateChangedEventArgs e)
        {
            datumKraja.MinimumDate = datumPocetka.Date;
     
        }
        private void PromjenaKraja(object sender, DateChangedEventArgs e)
        {
            datumPocetka.MaximumDate = datumKraja.Date;
        }
      
        private void Zavrsi(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            model.OsvjeziDogadjaj((VrstaDogadjaja)VrsteDog.SelectedItem);
            int g = 0;
        }
       
    }
}