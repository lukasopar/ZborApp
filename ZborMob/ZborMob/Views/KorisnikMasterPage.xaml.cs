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
    public partial class KorisnikMasterPage : ContentPage
    {
        public KorisnikMasterPage()
        {
            InitializeComponent();
            ime.Text = App.Korisnik.ImeIPrezimeP;
            slika.Source = "test.png";
            slika.Aspect = Aspect.AspectFit;
        }
    }
}