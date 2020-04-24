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
        public NovoPitanjePage()
        {
            model = new NovoPitanjeViewModel();
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
        private void Zavrsi(object sender, EventArgs e)
        {
            bool vis = true;
            if (Visestruki.SelectedItem.ToString().Length == 22)
                vis = false;
            model.Zavrsi(vis);
            Navigation.PopAsync();
        }
    }
}