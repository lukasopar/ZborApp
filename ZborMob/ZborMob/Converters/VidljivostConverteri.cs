using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using ZborDataStandard.Model;

namespace ZborMob.Converters
{
    public class ObavijestVidljivostConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (Obavijest)value;
            
            var hackyBindedKeyLabel = parameter as Label;
            var hackyBindedKeyValue = Boolean.Parse( hackyBindedKeyLabel.Text);
            return hackyBindedKeyValue || obj.IdKorisnik == App.Korisnik.Id;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PitanjeVidljivostConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (Anketa)value;

            var hackyBindedKeyLabel = parameter as Label;
            var hackyBindedKeyValue = Boolean.Parse(hackyBindedKeyLabel.Text);
            return hackyBindedKeyValue || obj.IdKorisnik == App.Korisnik.Id;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
