using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZborDataStandard.Model;
using ZborMob.Services;

namespace ZborMob.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KorisnikMainPage : MasterDetailPage
    {
        private HubConnection hubConnection;
        private ApiServices apiServices;
        private int _neprocitanePoruke;
        public KorisnikMainPage()
        {
            InitializeComponent();
            ToolbarItem item = new ToolbarItem
            {
                Text = "poruke",
                Order = ToolbarItemOrder.Primary,
                Priority = 0
            };

            // "this" refers to a Page object
            ToolbarItems.Add(item);
            masterPage.listView.ItemSelected += OnItemSelected;

            _neprocitanePoruke = 9;

            var uri = $"{App.BackendUrl}/" + "chatHub";
            hubConnection = new HubConnectionBuilder()
                .WithUrl(uri, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(App.Token);
                    options.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                            // bypass SSL certificate
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        return message;
                    };
                })
                .Build();
            Connect();
        }
        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            if (item != null)
            {
                Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
                masterPage.listView.SelectedItem = null;
                IsPresented = false;
            }
        }
        async Task Connect()
        {
            try
            {
                hubConnection.On<int>("Neprocitane", (broj) =>
                {
                    if(broj > 0)
                        ToolbarItems.Where(t => t.Text.Contains("poruke")).SingleOrDefault().Text = "nove poruke";
                    else
                        ToolbarItems.Where(t => t.Text.Contains("poruke")).SingleOrDefault().Text = "stare poruke";

                });
                hubConnection.On<Poruka>("ReceiveMessageMob", async (poruka) =>
                {
                    await hubConnection.SendAsync("NeprocitanePoruke");
                });
                hubConnection.On<Poruka>("ReceiveNewConversationMob", async (poruka) =>
                {
                    await hubConnection.SendAsync("NeprocitanePoruke");
                });
                await hubConnection.StartAsync();
                await hubConnection.SendAsync("NeprocitanePoruke");
                
            }
            catch (Exception e)
            {
                int g = 0;
            }

        }
    }
}