using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborMob.Converters;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KorisnikMasterPage : ContentPage
    {
        public KorisnikMasterPage()
        {
            InitializeComponent();
            ime.Text = App.Korisnik.ImeIPrezimeP;
            slika.SetBinding(Image.SourceProperty, new Binding(".", BindingMode.Default, new SlikaLinkConverter(), null));
            WebClient client = new WebClient();
            var link = App.BackendUrl + "/api/getrepozitorijkorisnik/" + App.Korisnik.IdSlika;
            var byteArray = client.DownloadData(link);
            slika.Source = ImageSource.FromStream(() => new MemoryStream(byteArray));
            // slika.Source = "test.png";
            slika.Aspect = Aspect.AspectFit;
        }
    }
}