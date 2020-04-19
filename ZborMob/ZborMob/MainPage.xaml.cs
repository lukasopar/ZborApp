using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ZborMob
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private HttpClient _httpClient;
        public HttpClient HttplicentAccount
        {
            get
            {
                _httpClient = _httpClient ?? new HttpClient
                (
                    new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                        {
                    //bypass
                    return true;
                        },
                    }
                    , false
                )
                {
                    BaseAddress = new Uri($"{App.BackendUrl}/"),
            };

                // In case you need to send an auth token...
                return _httpClient;
            }
        }
        public MainPage()
        {
            InitializeComponent();
        }
        public async void OnButtonClicked(object sender, EventArgs args)
        {


            HttplicentAccount.BaseAddress = new Uri($"{App.BackendUrl}/");
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(HttplicentAccount.BaseAddress, "api/pozdrav"),
                Method = HttpMethod.Get,
            };
            var poz = await HttplicentAccount.SendAsync(request);
            var label = new Label { Text = "poz" };
        }
    }
}
