using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using ZborDataStandard.Model;

namespace ZborMob.Converters
{
    class Converter
    {
    }
    public class ObavijestConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (Obavijest)value;
            if (obj.LajkObavijesti.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
                return "likes.png";
            else
                return "like.png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class KomentarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (KomentarObavijesti)value;
            if (obj.LajkKomentara.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
                return "likes.png";
            else
                return "like.png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NajavaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (Dogadjaj)value;
            if (obj.NajavaDolaska.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
                return Color.FromHex("1C6EBC");
            else
                return Color.Gray;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TekstConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (string)value;
            obj = obj.Replace("<b>", "");
            obj = obj.Replace("</b>", "");
            return obj;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
