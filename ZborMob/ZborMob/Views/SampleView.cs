using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ZborMob.Views
{
    public class SampleView : ContentView
    {
        public SampleView() { }

        public View PropertyView { get; set; }

        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }
    }
}
