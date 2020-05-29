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

        public KorisnikMainPage()
        {
            InitializeComponent();
            ToolbarItem item = new ToolbarItem
            {
                Text = "poruke",
                Order = ToolbarItemOrder.Primary,
                Priority = 0,
                IconImageSource ="envelope.png"
            };
            item.Clicked += async (sender, args) => await Navigation.PushModalAsync(new PorukePage()); 
            ToolbarItem item2 = new ToolbarItem
            {
                Text = "obavijest",
                Order = ToolbarItemOrder.Primary,
                Priority = 0,
                IconImageSource = "signs.png"
            };
            item2.Clicked += async (sender, args) => await Navigation.PushModalAsync(new ObavijestiKorisnikPage());
            // "this" refers to a Page object
            ToolbarItems.Add(item);
            ToolbarItems.Add(item2);
            masterPage.listView.ItemSelected += OnItemSelected;


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
                hubConnection.On<List<int>>("Neprocitane", (list) =>
                {
                    if (list.Count > 0)
                    {
                        ToolbarItems.Where(t => t.Text.Contains("poruke")).SingleOrDefault().IconImageSource = "envnew.png";
                    }
                    else
                    {
                        ToolbarItems.Where(t => t.Text.Contains("poruke")).SingleOrDefault().IconImageSource = "envelope.png";
                    }
                    DependencyService.Get<INotification>().DeleteNotification(list);
                });
                hubConnection.On<List<int>>("NeprocitaneObavijesti", (list) =>
                {
                    if (list.Count > 0)
                    {
                        ToolbarItems.Where(t => t.Text.Contains("obavijest")).SingleOrDefault().IconImageSource = "signsNew.png";
                    }
                    else
                    {
                        ToolbarItems.Where(t => t.Text.Contains("obavijest")).SingleOrDefault().IconImageSource = "signs.png";
                    }
                    DependencyService.Get<INotification>().DeleteNotification(list);
                });
                hubConnection.On<Poruka, string>("ReceiveMessageMob", async (poruka, ime) =>
                {
                    await hubConnection.SendAsync("NeprocitanePoruke");
                    DependencyService.Get<INotification>().CreateNotification(poruka.IdRazgovor, "Nova poruka: " + ime, poruka.Poruka1);
                });
                hubConnection.On<Razgovor, string>("ReceiveNewConversationMob", async (razg, ime) =>
                {
                    await hubConnection.SendAsync("NeprocitanePoruke");
                    DependencyService.Get<INotification>().CreateNotification(razg.Id, "Nova poruka: " + ime, razg.Poruka.First().Poruka1);
                });
                hubConnection.On<OsobneObavijesti>("NovaObavijest", async (ob) =>
                {
                    await hubConnection.SendAsync("NeprocitaneObavijesti");
                    var tekst = ob.Tekst.Replace("<b>", "");
                    tekst = tekst.Replace("</b>", "");
                    DependencyService.Get<INotification>().CreateNotification(ob.Id, "Nova obavijest: ", tekst);
                });
                await hubConnection.StartAsync();
                await hubConnection.SendAsync("NeprocitanePoruke");
                await hubConnection.SendAsync("NeprocitaneObavijesti");

            }
            catch (Exception e)
            {
                int g = 0;
            }

        }
    }
}