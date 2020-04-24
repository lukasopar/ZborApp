using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using ZborDataStandard.Model;
using Switch = Xamarin.Forms.Switch;

namespace ZborMob.Views
{
    public class CustomPitanjeCell : ViewCell
    {
        public static readonly BindableProperty PitanjeProperty =  BindableProperty.Create("Pitanje", typeof(Anketa), typeof(CustomPitanjeCell));

        public Dictionary<Guid, List<int>> KorisnickiOdgovori;
        public event EventHandler<EventArgs> PromjenaOdgovora;
        public event EventHandler<EventArgs> RezultatiClick;

        public Anketa Pitanje
        {
            get { return (Anketa)GetValue(PitanjeProperty); }
            set { SetValue(PitanjeProperty, value); }
        }
        public Label labelPitanje = new Label();
        public Picker picker = new Picker();
        public StackLayout switchers = new StackLayout();
        StackLayout verticalLayout = new StackLayout();
        public Button RezultatiBtn { get; set; } = new Button();


        public CustomPitanjeCell()
        {
            var pitanje = (Anketa)GetValue(PitanjeProperty);
            //instantiate each of our views
            StackLayout cellWrapper = new StackLayout();


            //Set properties for desired design
            verticalLayout.Orientation = StackOrientation.Vertical;
            

            //add views to the view hierarchy
            verticalLayout.Children.Add(labelPitanje);

            verticalLayout.Children.Add(picker);
            verticalLayout.Children.Add(switchers);
            RezultatiBtn.Text = "Rezultati";
            RezultatiBtn.Clicked += (sender, args) => this.RezultatiClick.Invoke(sender, args);
            verticalLayout.Children.Add(RezultatiBtn);
            cellWrapper.Children.Add(verticalLayout);
            View = cellWrapper;
        }

        
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            var pitanje = (Anketa)GetValue(PitanjeProperty);
            labelPitanje.Text = pitanje.Pitanje;
            RezultatiBtn.BindingContext = pitanje;

            if (pitanje.VisestrukiOdgovor == false)
            {
                picker.ItemsSource = pitanje.OdgovorAnkete.ToList();
                picker.ItemDisplayBinding = new Binding("Odgovor");
                var odg = pitanje.OdgovorAnkete.Where(o => o.OdgovorKorisnikaNaAnketu.Select(od => od.IdKorisnik).Contains(App.Korisnik.Id)).FirstOrDefault();
                if (odg != null)
                    picker.SelectedItem = odg;
                picker.SelectedIndexChanged +=  (sender, args) => this.PromjenaOdgovora.Invoke(this, args);
            }
            else
            {
                verticalLayout.Children.Remove(picker);
                foreach(var odg in pitanje.OdgovorAnkete)
                {
                    StackLayout horizontalLayout = new StackLayout();
                    horizontalLayout.Orientation = StackOrientation.Horizontal;
                    Label l = new Label { Text = odg.Odgovor };
                    Switch cell = new Switch();
                    if (odg.OdgovorKorisnikaNaAnketu.Select(o => o.IdKorisnik).Contains(App.Korisnik.Id))
                        cell.IsToggled = true;
                    horizontalLayout.Children.Add(l);
                    horizontalLayout.Children.Add(cell);
                    cell.BindingContext = odg;
                    cell.Toggled += (sender, args) => this.PromjenaOdgovora.Invoke(this, args);
                    switchers.Children.Add(horizontalLayout);
                    
                }
            }


        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
