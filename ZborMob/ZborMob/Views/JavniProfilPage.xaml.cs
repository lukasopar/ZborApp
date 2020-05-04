using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborMob.ViewModels;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JavniProfilPage : ContentPage
    {
        private JavniProfilViewModel model;
        public JavniProfilPage()
        {
            model = new JavniProfilViewModel(App.Zbor.Id);
            BindingContext = model;
            InitializeComponent();
            
        }
       public void SpremiOZboru(object o, EventArgs args)
        {
            model.SpremiOZboru(oZboru.Html);
        }
        public void SpremiOVoditeljima(object o, EventArgs args)
        {
            model.SpremiOVoditeljima(oVoditeljima.Html);
        }
        public void SpremiRepertoar(object o, EventArgs args)
        {
            model.SpremiRepertoar(repertoar.Html);
        }
        public void SpremiReprezentacija(object o, EventArgs args)
        {
            model.SpremiReprezentacija(reprezentacija.Html);
        }
    }
}