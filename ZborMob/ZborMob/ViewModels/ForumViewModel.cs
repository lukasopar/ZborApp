using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.ForumViewModels;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class ForumViewModel : INotifyPropertyChanged
    {
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<KategorijaForuma> kategorije;
        public ObservableCollection<KategorijaForuma> Kategorije
        {
            get
            {
                return kategorije;
            }
            set
            {
                kategorije = value;
                RaisepropertyChanged("Kategorije");
            }
        }

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

        private bool mod;
        public bool Mod
        {
            get
            {
                return mod;
            }
            set
            {
                mod = value;
                RaisepropertyChanged("Mod");
            }
        }
        private IndexViewModel model;

        public ForumViewModel()
        {
            _apiServices = new ApiServices();
            IsBusy = true;
            GetData();

        }
        async void GetData()
        {
            model = await _apiServices.Forum();
            IsBusy = false;
            Kategorije = new ObservableCollection<KategorijaForuma>(model.KategorijaForuma);
            
        }
       
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
