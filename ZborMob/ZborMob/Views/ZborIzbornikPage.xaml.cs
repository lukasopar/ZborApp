using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ZborIzbornikPage : ContentPage
    {
        public ZborIzbornikPage()
        {
            InitializeComponent();
            ime.Text = App.Zbor.Naziv;
            slika.Source = "test.png";
            slika.Aspect = Aspect.AspectFit;
        }
    }
}