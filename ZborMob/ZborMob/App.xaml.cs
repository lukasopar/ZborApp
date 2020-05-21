using Newtonsoft.Json;
using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborDataStandard.Model;
using ZborMob.Views;

namespace ZborMob
{
    public partial class App : Application
    {
        public static string BaseImageUrl { get; } = "https://cdn.syncfusion.com/essential-ui-kit-for-xamarin.forms/common/uikitimages/";
        public static string IPAddress = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
        public static string BackendUrl = $"https://{IPAddress}:5001";
        public static string Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFhQG1haWwuY29tIiwibmFtZWlkIjoiNmViYjg2ZGMtZDMxNi00Nzk4LTJmNTAtMDhkN2M1Y2U3Y2JjIiwibmJmIjoxNTg5NTM4NTUyLCJleHAiOjE1OTAxNDMzNTIsImlhdCI6MTU4OTUzODU1Mn0.iP37EGuiqOO54-sEEkmd4Hx_inF1nIcGtgwxHDpUnOg";
        public static Korisnik Korisnik = new Korisnik {};
        public static Zbor Zbor = new Zbor();
        public App()
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "korisnik.txt");
            bool doesExist = File.Exists(fileName);
            if(!doesExist)
            {
                using (File.Create(fileName)) { }
                //Login
            }

            string tokenFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "token.txt");
            bool doesExistToken = File.Exists(tokenFileName);
            if (!doesExistToken)
            {
                using (File.Create(tokenFileName)) { }
                //Login
            }


            //Korisnik.Id = Guid.Parse("6ebb86dc-d316-4798-2f50-08d7c5ce7cbc");
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjQ5OTM0QDMxMzgyZTMxMmUzMG9hOXpiZHZhV3pkaWp5Vm5lbmdDeGFGY1BDYUZiQ3JzYVhWNE1qbis5U3M9");
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                System.Exception ex = (System.Exception)args.ExceptionObject;
                Console.WriteLine(ex);
            };

            Token = File.ReadAllText(tokenFileName);
            if (!doesExist || Token == "")
            {
                MainPage = new LoginPage();
            }
            else
            {
                var content = File.ReadAllText(fileName);
                Korisnik = JsonConvert.DeserializeObject<Korisnik>(content);
                MainPage = new KorisnikMainPage();
            }
            
        }

        public  void UspjesanLogin()
        {
            MainPage = new KorisnikMainPage();
        }
        public void NeuspjesanLogin()
        {

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
