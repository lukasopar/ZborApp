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
    public partial class KalendarPage : ContentPage
    {
        public KalendarPage()
        {
            BindingContext = new KalendarViewModel();
            InitializeComponent();
        }
    }
}