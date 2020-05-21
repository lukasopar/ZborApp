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
    public partial class ForumPage : ContentPage
    {
        ForumViewModel model;
        public ForumPage()
        {
            model = new ForumViewModel();
            BindingContext = model;
            InitializeComponent();
        }

        private void Teme(object sender, SelectedItemChangedEventArgs e)
        {
            Navigation.PushAsync(new TemePage((e.SelectedItem as Forum).Id));
        }

        
    }
    
}