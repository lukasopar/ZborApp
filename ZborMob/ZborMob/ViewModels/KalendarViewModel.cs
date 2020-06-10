using Syncfusion.SfCalendar.XForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class KalendarViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public CalendarEventCollection CalendarInlineEvents { get; set; } = new CalendarEventCollection();
        private ApiServices _apiServices = new ApiServices();


        private bool isBusy;
        public bool IsBusy
        {
            get
            {
                return isBusy;
            }
            set
            {
                isBusy = value;
                RaisepropertyChanged("IsBusy");
            }
        }
        public KalendarViewModel()
        {
            IsBusy = true;
            GetData();
        }
        private async void GetData()
        {
            var model = await _apiServices.Kalendar(App.Zbor.Id);
            foreach(var d in model.Dogadjaji)
            {
                var colors = d.BackgroundColor.Substring(4, d.BackgroundColor.Length - 5).Split(",");
                int r = Int32.Parse(colors[0]);
                int g = Int32.Parse(colors[1]);
                int b = Int32.Parse(colors[2]);

                var ev = new CalendarInlineEvent()
                {
                    StartTime = DateTime.Parse(d.Start),
                    EndTime = DateTime.Parse(d.End),
                    Subject = d.Title,
                    Color = Color.FromRgb(r,g,b)
                };
                CalendarInlineEvents.Add(ev);
            }
            IsBusy = false;
        }

        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
