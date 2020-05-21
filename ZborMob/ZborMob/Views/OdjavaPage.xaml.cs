using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OdjavaPage : ContentPage
    {
        public OdjavaPage()
        {
            InitializeComponent();
            string tokenFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "token.txt");
            File.WriteAllText(tokenFileName, "");
            App.Token = "";
            App.Current.MainPage = new LoginPage();
        }
    }
}