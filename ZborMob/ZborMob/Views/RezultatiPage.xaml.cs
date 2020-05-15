
using Syncfusion.SfChart.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborDataStandard.Model;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RezultatiPage : ContentPage
    {
        public ObservableCollection<ChartDataPoint> Podaci { get; set; }

        public RezultatiPage(Anketa pitanje)
        {
            //var random = new Random();
            Podaci = new ObservableCollection<ChartDataPoint>();

            foreach (var odg in pitanje.OdgovorAnkete)
            {
                var broj = odg.OdgovorKorisnikaNaAnketu.Count();
                //  var color = String.Format("#{0:X6}", random.Next(0x1000000));
                Podaci.Add(new ChartDataPoint("Odgovor: " + odg.Odgovor, broj));
                
            }
            BindingContext = Podaci;            
         
            InitializeComponent();
        //    this.chartView.Chart = chart;
            this.Pitanje.Text = pitanje.Pitanje;
            

        }
    }
}