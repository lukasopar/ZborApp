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
    public partial class DodajZborPage : ContentPage
    {
        private DodajZborViewModel model;
        private PocetnaViewModel popis;
        public DodajZborPage(PocetnaViewModel viewmodel)
        {
            model = new DodajZborViewModel();
            BindingContext = model;
            InitializeComponent();
            datum.MaximumDate = DateTime.Now;
            popis = viewmodel;
        }
        public async void Dodaj(object o, EventArgs e)
        {
            var zbor  = await model.Dodaj();
            popis.MojiZborovi.Add(zbor);
            await Navigation.PopAsync();
        }
        
    }
}