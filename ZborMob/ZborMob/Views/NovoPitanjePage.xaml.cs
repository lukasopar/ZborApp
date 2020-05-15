using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborMob.ViewModels;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NovoPitanjePage : ContentPage
    {
        private NovoPitanjeViewModel model;
        PitanjaViewModel stariModel;
        public NovoPitanjePage(PitanjaViewModel stariModel)
        {
            model = new NovoPitanjeViewModel();
            this.stariModel = stariModel;
            this.BindingContext = model;
            InitializeComponent();
            datumPicker.MinimumDate = DateTime.Now;
            Visestruki.SelectedItem = "Odabir jednog odgovora";
        }
        private void DodajOdgovor(object o, EventArgs e)
        {
            model.Odgovori.Add(new StringValue(""));
        }
          private void ObrisiOdgovor(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            model.Odgovori.Remove((StringValue)btn.BindingContext);
        }
        private async void Zavrsi(object sender, EventArgs e)
        {
            bool vis = true;
            if (Visestruki.SelectedItem == null || model.Datum == null || model.Pitanje == null || model.Pitanje.Trim() == "")
            {
                await DisplayAlert("Greška", "Ispunite sva polja za dodavanje pitanja", "Ok");
                return;

            }
            if (Visestruki.SelectedItem.ToString().Length == 22)
                vis = false;
            try
            {
                var novo = await model.Zavrsi(vis);
                stariModel.Ankete.Insert(0, novo);
            }catch(ArgumentException)
            {
                await DisplayAlert("Greška", "Ispunite sva polja za dodavanje pitanja", "Ok");
                return;
            }
            await Navigation.PopModalAsync();
        }
    }
}