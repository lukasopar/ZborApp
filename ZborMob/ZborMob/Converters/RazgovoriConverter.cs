using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Xamarin.Forms;
using ZborDataStandard.Model;

namespace ZborMob.Converters
{
   
    public class NazivRazgovoraConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (Razgovor)value;
            string naslov = obj.GetNaslov();
            string clanovi = obj.GetPopisKorisnika(App.Korisnik.Id);
            if (naslov.Equals("Razgovor"))
                return clanovi;
            else
                return naslov + "(" + clanovi + ")";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PorukaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (Razgovor)value;
            var poruka  = obj.Poruka.First();
            if (poruka.IdKorisnik == App.Korisnik.Id)
                return "Ti: " + poruka.Poruka1;
            else return poruka.Poruka1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SlikaConverter : IValueConverter
    {
        static WebClient Client = new WebClient();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (Razgovor)value;
            var poruka = obj.Poruka.First();
            string slika = "";
            foreach(var k in obj.KorisnikUrazgovoru)
            {
                if (k.IdKorisnik != App.Korisnik.Id)
                    slika = k.IdKorisnikNavigation.IdSlika.ToString();
            }
            var link= App.BackendUrl + "/api/getrepozitorijkorisnik/" + slika;
            var byteArray = Client.DownloadData(link);
            return  ImageSource.FromStream(() => new MemoryStream(byteArray));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SlikaUPorukamaConverter : IValueConverter
    {
        static WebClient Client = new WebClient();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var obj = (string)value;

           
            var byteArray = Client.DownloadData(obj);
            return ImageSource.FromStream(() => new MemoryStream(byteArray));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
