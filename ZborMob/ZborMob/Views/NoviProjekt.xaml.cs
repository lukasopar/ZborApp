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
    public partial class NoviProjekt : ContentPage
    {
        ProjektiViewModel model;
        public NoviProjekt(ProjektiViewModel model)
        {
            this.model = model;
            BindingContext = model;
            InitializeComponent();
            datePicker.MinimumDate = DateTime.Now;

        }
        private void Dodaj(object sender, EventArgs e)
        {
            var vrsta = (VrstaPodjele)picker.SelectedItem;
            model.NoviProjekt(vrsta);
        }
    }
}