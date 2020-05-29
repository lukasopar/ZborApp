using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            WebClient client = new WebClient();
            var link = App.BackendUrl + "/api/getrepozitorijzbor/" + App.Zbor.IdSlika;
            var byteArray = client.DownloadData(link);
            slika.Source = ImageSource.FromStream(() => new MemoryStream(byteArray)); slika.Aspect = Aspect.AspectFit;
        }
    }
}