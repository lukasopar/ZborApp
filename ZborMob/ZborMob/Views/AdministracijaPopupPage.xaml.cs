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
    public partial class AdministracijaPopupPage : PopupPage
    {
        private ClanZbora clan;
        private AdministracijaViewModel model;
        private int index;
        public AdministracijaPopupPage(AdministracijaViewModel model, ClanZbora clan)
        {
            this.model = model;
            this.clan = clan;
            

            if (clan.Glas.Equals("sopran"))
                index = 0;
            if (clan.Glas.Equals("alt"))
                index = 1;
            if (clan.Glas.Equals("tenor"))
                index = 2;
            if (clan.Glas.Equals("bas"))
                index = 3;
            InitializeComponent();
        }

        private async void Izbaci(object sender, EventArgs e)
        {

            model.Izbaci(clan);
            await Navigation.PopPopupAsync();
        }
        private async void PostaviModeratora(object sender, EventArgs e)
        {

            model.PostaviModeratora(clan);
            await Navigation.PopPopupAsync();


        }
        private async void PostaviVoditelja(object sender, EventArgs e)
        {

            model.PostaviVoditelja(clan);
            await Navigation.PopPopupAsync();


        }
        private async void PromjeniGlas(object sender, EventArgs e)
        {
            int glas = picker.SelectedIndex;
            model.PromjenaGlasa(clan, index, glas);
            await Navigation.PopPopupAsync();

        }
    }
}