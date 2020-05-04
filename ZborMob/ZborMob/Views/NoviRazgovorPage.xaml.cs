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
    public partial class NoviRazgovorPage : ContentPage
    {
        NoviRazgovorViewModel model;
        PorukeViewModel viewmodel;
        public NoviRazgovorPage(PorukeViewModel viewmodel)
        {
            model = new NoviRazgovorViewModel();
            BindingContext = model;
            this.viewmodel = viewmodel;
            InitializeComponent();
        }

        private void Posalji(object sender, EventArgs e)
        {
            var selected = autoComplete.SelectedIndices as List<int>;
            var list = new List<string>();
            foreach(var index in selected)
            {
                list.Add(model.Korisnici[index].Id.ToString());
            }
            viewmodel.Posalji(model.NewText, list) ;
            Navigation.PopModalAsync();
        }
    }
}