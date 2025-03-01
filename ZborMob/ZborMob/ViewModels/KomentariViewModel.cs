﻿using System;
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
    public class KomentariViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<KomentarObavijesti> komentari;

        public Guid IdKorisnik { get; set; }

        public ObservableCollection<KomentarObavijesti> Komentari
        {
            get
            {
                return komentari;
            }
            set
            {
                komentari = value;
                RaisepropertyChanged("Komentari");
            }
        }
        private bool isEmpty;
        public bool IsEmpty
        {
            get
            {
                return isEmpty;
            }
            set
            {
                isEmpty = value;
                RaisepropertyChanged("IsEmpty");
            }
        }

        private Obavijest model;
        public string Novi { get; set; }
        public KomentariViewModel(Obavijest o)
        {
            _apiServices = new ApiServices();
            model = o;
            IsEmpty = true;
            Komentari = new ObservableCollection<KomentarObavijesti>(model.KomentarObavijesti);
            IsEmpty =!( Komentari.Count > 0);
        }
       
        public async Task Lajk(KomentarObavijesti o)
        {
            if (!o.LajkKomentara.Select(l => l.IdKorisnik).Contains(App.Korisnik.Id))
            {
                await _apiServices.LajkKomentaraAsync(o.Id);
                o.LajkKomentara.Add(new LajkKomentara { Id = Guid.NewGuid(), IdKorisnik = App.Korisnik.Id, IdKomentar = o.Id });

            }
            else
            {
                var l = o.LajkKomentara.Where(l => l.IdKorisnik == App.Korisnik.Id && l.IdKomentar == o.Id).SingleOrDefault();
                o.LajkKomentara.Remove(l);
                await _apiServices.UnLajkKomentaraAsync(o.Id);
            }
            RaisepropertyChanged("Komentari");

        }
        public async Task  DodajNovi()
        {
            var kom = await _apiServices.NoviKomentarAsync(Novi, model.Id);
            IsEmpty = false;
            Komentari.Add(kom);
            IsEmpty = false;
            Novi = "";

        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
