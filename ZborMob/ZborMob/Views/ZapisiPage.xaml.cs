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
    public partial class ZapisiPage : ContentPage
    {
        ZapisiViewModel model;
        public ZapisiPage(Guid id)
        {
            model = new ZapisiViewModel(id);
            BindingContext = model;
            InitializeComponent();
        
        }
        public async void Promjena(object o, EventArgs e)
        {
            int stranica = ((PagerView)o).Trenutno;
            await model.Promjena(stranica);
        }
        public async void Dodaj(object o, EventArgs e)
        {
            if (model.Novi.Tekst.Trim() == "")
            {
                await DisplayAlert("Greška", "Unesite zapis", "Ok");
                return;
            }
            await model.Dodaj();
            pager.StaviZadnju();   
        }
        public void Citiraj(object o, EventArgs e)
        {
            var btn = (Button)o;
            var ime = (Zapis)btn.BindingContext;
            var tekst = model.Novi.Tekst + "<blockquote><b>" + ime.IdKorisnikNavigation.ImeIPrezimeP + "</b> kaže:<p>" + ime.Tekst + "</p></blockquote><br />";
            model.Novi = new Zapis { Tekst = tekst };
        }
        public void Spremi(object o, EventArgs e)
        {
            var editor = (TEditorHtmlView)o;
            var zapis = editor.BindingContext as Zapis;
            model.Spremi(editor.Html, zapis);
        }
        public void Obrisi(object o, EventArgs e)
        {
            var btn = (Button)o;
            var zapis = btn.BindingContext as Zapis;
            model.ObrisiZapis(zapis, pager.Trenutno);
        }
    }
}