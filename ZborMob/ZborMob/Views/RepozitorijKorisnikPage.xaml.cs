using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborDataStandard.Model;
using ZborMob.ViewModels;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RepozitorijKorisnikPage : ContentPage
    {
        RepozitorijKorisnikViewModel model;
        public RepozitorijKorisnikPage()
        {
            model = new RepozitorijKorisnikViewModel(App.Korisnik.Id);
            BindingContext = model;
            InitializeComponent();
        }

        private void Vidljivost(object sender, ToggledEventArgs e)
        {
            var src = (Switch)sender;
            model.Vidljivost((RepozitorijKorisnik)src.BindingContext);
        }
        private void Obrisi(object sender, EventArgs e)
        {
            var src = (Button)sender;
            model.Obrisi((RepozitorijKorisnik)src.BindingContext);
        }
        private async void Preuzmi(object sender, EventArgs e)
        {
            var src = (Label)sender;
            model.Preuzmi((RepozitorijKorisnik)src.BindingContext);
            await DisplayAlert("Preuzimanje", "Datoteka je spremljena u direktorij Preuzimanja.", "OK");

        }
        private async void Podijeli(object sender, EventArgs e)
        {
            var src = (Button)sender;
            await Clipboard.SetTextAsync("https://localhost:5001/Repozitorij/Get" + ((RepozitorijZbor)src.BindingContext).Url);
            await DisplayAlert("Kopirano u međuspremnik","Kopirano", "OK");

        }
        private async void Upload(object sender, EventArgs e)
        {
            try
            {
                FileData fileData = await CrossFilePicker.Current.PickFile();

                if (fileData == null)
                    return; // user canceled file picking

                model.Upload(fileData);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception choosing file: " + ex.ToString());
            }
        }
    }
}