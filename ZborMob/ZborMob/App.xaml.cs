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
        public static string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFhQG1haWwuY29tIiwibmFtZWlkIjoiNmViYjg2ZGMtZDMxNi00Nzk4LTJmNTAtMDhkN2M1Y2U3Y2JjIiwibmJmIjoxNTg4MjUwMzQ1LCJleHAiOjE1ODg4NTUxNDUsImlhdCI6MTU4ODI1MDM0NX0.AyizXlHcoof8_9eyP4pfizIQ7YG84VjeEdDvarUe6UY";
        public static Korisnik Korisnik = new Korisnik();
        public static Zbor Zbor = new Zbor();
        public App()
        {
            Korisnik.Id = Guid.Parse("6ebb86dc-d316-4798-2f50-08d7c5ce7cbc");
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjQ5OTM0QDMxMzgyZTMxMmUzMG9hOXpiZHZhV3pkaWp5Vm5lbmdDeGFGY1BDYUZiQ3JzYVhWNE1qbis5U3M9");
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                System.Exception ex = (System.Exception)args.ExceptionObject;
                Console.WriteLine(ex);
            };
            MainPage = new KorisnikMainPage();
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
