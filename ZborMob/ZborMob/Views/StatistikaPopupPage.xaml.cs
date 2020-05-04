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
    public partial class StatistikaPopupPage : PopupPage
    {
        StatistikaViewModel model;
        public StatistikaPopupPage(ClanNaProjektu clan)
        {

            this.model = new StatistikaViewModel(clan.IdKorisnik, clan.IdProjekt);
            this.BindingContext = model;
            InitializeComponent();
        }

       
       
    }
}