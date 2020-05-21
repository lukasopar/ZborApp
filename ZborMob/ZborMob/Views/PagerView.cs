using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ZborMob.Views
{
    public class PagerView : Grid
    {
        public static readonly BindableProperty UkupnoProperty = BindableProperty.Create("Ukupno", typeof(int), typeof(PagerView));
        public event EventHandler<EventArgs> PromjenaStranice;

        public int Ukupno
        {
            get { return (int)GetValue(UkupnoProperty); }
            set { SetValue(UkupnoProperty, value); }
        }
        public int Trenutno { get { return trenutno; } set { } }
        private int ukupno = 1;
        private int trenutno = 1;
        public Button Natrag;
        public Button Naprijed;
        public Label Tekst;
        public PagerView()
        {
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Natrag = new Button
            {
                Text = "Natrag",
                IsEnabled = false
            };
            Natrag.Clicked += Natrag_Clicked;
            Naprijed = new Button
            {
                Text = "Naprijed",
                IsEnabled = false
            };
            Naprijed.Clicked += Naprijed_Clicked;
            Tekst = new Label
            {
                Text = trenutno + "/" + ukupno,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 15
            };
            Children.Add(Natrag, 0, 0);
            Children.Add(Tekst, 1, 0);
            Children.Add(Naprijed, 2, 0);

        }

        private void Natrag_Clicked(object sender, EventArgs e)
        {
            trenutno--;
            var uk = (int)GetValue(UkupnoProperty);
            Tekst.Text = trenutno + "/" + uk;
            if (trenutno == 1)
                Natrag.IsEnabled = false;
            else
                Natrag.IsEnabled = true;
            if (trenutno == uk)
                Naprijed.IsEnabled = false;
            else
                Naprijed.IsEnabled = true;
            PromjenaStranice.Invoke(this, e);
        }
        public void StaviZadnju()
        {
            
            var uk = (int)GetValue(UkupnoProperty);
            trenutno = uk;
            Tekst.Text = trenutno + "/" + uk;
            if (trenutno == 1)
                Natrag.IsEnabled = false;
            else
                Natrag.IsEnabled = true;
            if (trenutno == uk)
                Naprijed.IsEnabled = false;
            else
                Naprijed.IsEnabled = true;

        }
        private void Naprijed_Clicked(object sender, EventArgs e)
        {
            trenutno++;
            var uk = (int)GetValue(UkupnoProperty);
            Tekst.Text = trenutno + "/" + uk;
            if (trenutno == 1)
                Natrag.IsEnabled = false;
            else
                Natrag.IsEnabled = true;
            if (trenutno == uk)
                Naprijed.IsEnabled = false;
            else
                Naprijed.IsEnabled = true;
            PromjenaStranice.Invoke(this, e);

        }
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName ==  UkupnoProperty.PropertyName)
            {
                var uk = (int)GetValue(UkupnoProperty);
                Tekst.Text = trenutno + "/" + uk;
                if (trenutno == 1)
                    Natrag.IsEnabled = false;
                else
                    Natrag.IsEnabled = true;
                if (trenutno == uk)
                    Naprijed.IsEnabled = false;
                else
                    Naprijed.IsEnabled = true;
            }
          
        }
       
        
    }
}
