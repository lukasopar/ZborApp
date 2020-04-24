using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
        public RezultatiPage(Anketa pitanje)
        {
            var entries = new List<Microcharts.Entry>();
            var random = new Random();

            foreach (var odg in pitanje.OdgovorAnkete)
            {
                var broj = odg.OdgovorKorisnikaNaAnketu.Count();
                var color = String.Format("#{0:X6}", random.Next(0x1000000));
                entries.Add(new Microcharts.Entry(broj)
                {
                    Label = "Odgovor: " + odg.Odgovor,
                    ValueLabel = "Broj odgovora: " + broj + "",
                    Color = SKColor.Parse(color)
                }) ;
            }
                        
            var chart = new BarChart() { Entries = entries };
            chart.LabelTextSize = 30;
         
            InitializeComponent();
            this.chartView.Chart = chart;
            this.Pitanje.Text = pitanje.Pitanje;
            

        }
    }
}