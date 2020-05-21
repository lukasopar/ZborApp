using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using ZborDataStandard.Model;

namespace ZborMob.Converters
{
    public class ZadnjaTemaForumaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (Forum)value;

            var zadnja = obj.ZadnjaTema();
            if (zadnja == null)
                return "Nema";
            return zadnja.ZadnjiZapis.ToString("dd.MM.yyyy. HH:mm");


        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
  

}
