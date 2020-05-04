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
    public partial class RazgovorPage : ContentPage
    {
        RazgovorViewModel model;
        public RazgovorPage(Razgovor razgovor, PorukeViewModel model)
        {
            this.model = new RazgovorViewModel(razgovor);
            BindingContext = model;
            InitializeComponent();
        }
    }
}