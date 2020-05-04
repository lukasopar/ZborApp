using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborDataStandard.Model;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PocetnaPage : ContentPage
    {
        public ListView ListView { get; set; } = new ListView();
        public ObservableCollection<Zbor> ListViewData { get; set; } = new ObservableCollection<Zbor>();

        public PocetnaPage()
        {
            InitializeComponent();
           
        }
        async void MojZborTapped(object sender, SelectedItemChangedEventArgs e)
        {
            ListView btn = (ListView)sender;
            Zbor zbor = (Zbor)btn.SelectedItem;
            App.Zbor = zbor;
            await Navigation.PushAsync(new ZborMasterPage());
        }
    }
}