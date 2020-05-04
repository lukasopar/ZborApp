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
    public partial class PorukePage : ContentPage
    {
        PorukeViewModel model;
        public PorukePage()
        {
            model = new PorukeViewModel();
            BindingContext = model;
            InitializeComponent();
        }
    
        private async void Poruka (object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            var item = (Razgovor)e.ItemData;
            await Navigation.PushModalAsync(new DataTemplateSelector(item));
        }
        private async void NoviRazgovor(object o, EventArgs arg)
        {
            await Navigation.PushModalAsync(new NoviRazgovorPage(model));
        }
    }
}