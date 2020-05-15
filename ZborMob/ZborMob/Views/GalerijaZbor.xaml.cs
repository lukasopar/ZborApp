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
    public partial class GalerijaZbor : ContentPage
    {
        GalerijaZborViewModel model;
        public GalerijaZbor(Guid id)
        {
            model = new GalerijaZborViewModel(id);
            BindingContext = model;
            InitializeComponent();
        }
        public async void Profilna(object o, EventArgs e)
        {
            var btn = (Button)o;
            model.Profilna(((RepozitorijZbor)btn.BindingContext).Id);
            App.Zbor.IdSlika = ((RepozitorijZbor)btn.BindingContext).Id;
        }
    }
}