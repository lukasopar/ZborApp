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
    public partial class PitanjaPage : ContentPage
    {
        PitanjaViewModel model;
        public PitanjaPage()
        {
            model = new PitanjaViewModel();
            this.BindingContext = model;
            InitializeComponent();
        }
        private void PromjenaOdgovora(object o, EventArgs e)
        {
            var pitanje = (Anketa)((CustomPitanjeCell)o).Pitanje;
            var list = new List<int>();
            if(pitanje.VisestrukiOdgovor)
            {
                var hls = (((CustomPitanjeCell)o).switchers.Children);
                foreach(var sw in hls)
                {
                    var hl = ((StackLayout)sw).Children.Last();
                    try
                    {
                        if (((Switch)hl).IsToggled)
                            list.Add(((OdgovorAnkete)hl.BindingContext).Redoslijed);
                    }
                    catch(InvalidCastException)
                    {

                    }
                    
                }
            }
            else
            {
                var picker = (Picker)((CustomPitanjeCell)o).picker;
                list.Add(((OdgovorAnkete)picker.SelectedItem).Redoslijed);
            }
            model.OdgovoriNaPitanje(pitanje.Id, list);
            
        }
        private void Rezultati(object o, EventArgs e)
        {
            var pitanje = (Anketa)((Button)o).BindingContext;

            Navigation.PushModalAsync(new RezultatiPage(pitanje));
        }
        private void NovoPitanje(object o, EventArgs e)
        {
            
            Navigation.PushModalAsync(new NovoPitanjePage());
        }
    }
}