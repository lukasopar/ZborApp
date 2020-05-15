using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.ZborViewModels;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class NovoPitanjeViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public string Pitanje { get; set; }
        public DateTime Datum { get; set; }
        public bool Visestruki { get; set; }
        private ObservableCollection<StringValue> odgovori;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<StringValue> Odgovori
        {
            get
            {
                return odgovori;
            }
            set
            {
                odgovori = value;
                RaisePropertyChanged("Odgovori");

            }
        }

        public NovoPitanjeViewModel()
        {
            _apiServices = new ApiServices();
            odgovori = new ObservableCollection<StringValue>();
            odgovori.Add(new StringValue(""));
            odgovori.Add(new StringValue(""));
            odgovori.Add(new StringValue(""));

        }

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public async Task<Anketa> Zavrsi(bool vis)
        {
            Anketa pitanje = new Anketa
            {
                IdZbor = App.Zbor.Id,
                VisestrukiOdgovor = vis,
                DatumKraja = Datum,
                Pitanje = Pitanje
            };
            int i = 0;
            foreach(var odgovor in Odgovori)
            {
                if (odgovor.Value.Trim() == "")
                    throw new ArgumentException();
                pitanje.OdgovorAnkete.Add(new OdgovorAnkete
                {
                    Odgovor = odgovor.Value,
                    Redoslijed = i
                });
                i++;
            }
            var novo = await _apiServices.NovoPitanjeAsync(App.Zbor.Id, pitanje);
            return novo;
        }
    }
    public class StringValue
    {
        public string Value { get; set; }
        public StringValue(string value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            return Value.Equals(((StringValue)obj).Value);
        }
    }
}
