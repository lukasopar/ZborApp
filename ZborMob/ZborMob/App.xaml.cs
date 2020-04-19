using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborDataStandard.Model;
using ZborMob.Views;

namespace ZborMob
{
    public partial class App : Application
    {
        public static string IPAddress = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
        public static string BackendUrl = $"https://{IPAddress}:5001";
        public static string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjZlYmI4NmRjLWQzMTYtNDc5OC0yZjUwLTA4ZDdjNWNlN2NiYyIsIm5iZiI6MTU4NzIyNTAwMiwiZXhwIjoxNTg3ODI5ODAyLCJpYXQiOjE1ODcyMjUwMDJ9.DY-QoDHvA9MhD5cuB3ionU75SLwvnEgn7oCjjBwx4JA";
        public static Korisnik Korisnik;
        public App()
        {
            InitializeComponent();

            MainPage = new LoginPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
