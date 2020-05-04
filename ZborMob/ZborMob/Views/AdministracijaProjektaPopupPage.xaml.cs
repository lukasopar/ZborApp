using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
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
    public partial class AdministracijaProjektaPopupPage : PopupPage
    {
        private ClanNaProjektu clan;
        private AdministracijaProjektaViewModel model;
        public AdministracijaProjektaPopupPage(AdministracijaProjektaViewModel model, ClanNaProjektu clan)
        {
            this.model = model;
            this.clan = clan;
          
            InitializeComponent();
            var picker = new Picker { };
            picker.ItemsSource = model.Projekt.IdVrstePodjeleNavigation.Glasovi();
            picker.SelectedIndexChanged += PromjeniGlas;
            layout.Children.Add(picker);
        }

        private async void Izbaci(object sender, EventArgs e)
        {

            model.Izbaci(clan);
            await Navigation.PopPopupAsync();
        }
       
        private async void PromjeniGlas(object sender, EventArgs e)
        {
            var picker = sender as Picker;
            var selectedValue = picker.Items[picker.SelectedIndex];

            
            model.PromjenaGlasa(clan, selectedValue );
            await Navigation.PopPopupAsync();

        }
    }
}