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
    public partial class TemePage : ContentPage
    {
        TemeViewModel model;
        public TemePage(Guid id)
        {
            model = new TemeViewModel(id);
            BindingContext = model;
            InitializeComponent();
        
        }
        public void Promjena(object o, EventArgs e)
        {
            int stranica = ((PagerView)o).Trenutno;
            model.Promjena(stranica);
        }
        public void Zapis(object o, SelectedItemChangedEventArgs e)
        {
            Navigation.PushAsync(new ZapisiPage((e.SelectedItem as Tema).Id));
        }
        public void Dodaj(object o, EventArgs e)
        {
            model.Dodaj();
        }
        public void Obrisi(object o, EventArgs e)
        {
            var btn = (Button)o;
            var tema = btn.BindingContext as Tema;
            model.ObrisiTema(tema, pager.Trenutno);
        }
       
    }
}