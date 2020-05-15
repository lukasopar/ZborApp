using Microsoft.AspNetCore.SignalR.Client;
using Syncfusion.ListView.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.JSONModels;
using ZborMob.Helpers;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class RazgovorViewModel : INotifyPropertyChanged
    {
        private HubConnection hubConnection;
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<MessageInfo> messageInfo;
        private string newText;
        private string sendIcon;
        public ObservableCollection<MessageInfo> MessageInfo
        {
            get { return messageInfo; }
            set { this.messageInfo = value; RaisepropertyChanged("MessageInfo"); }
        }

        public string NewText
        {
            get { return newText; }
            set
            {
                newText = value;
                RaisepropertyChanged("NewText");
            }
        }
        public string korisnici;
        public string Korisnici
        {
            get { return korisnici; }
            set
            {
                korisnici = value;
                RaisepropertyChanged("Korisnici");
            }
        }
        public string SendIcon
        {
            get
            { return sendIcon; }
            set
            { sendIcon = value; }
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
        public Command<object> SendCommand { get; set; }

        public Command<object> LoadCommand { get; set; }
        private Razgovor razgovor;
        public RazgovorViewModel(Razgovor razgovor)
        {
            InitializeSendCommand();
            this.razgovor = razgovor;
            IsBusy = true;

            Korisnici = razgovor.GetPopisKorisnika(App.Korisnik.Id);
            _apiServices = new ApiServices();
            var uri = $"{App.BackendUrl}/" + "chatHub";
            hubConnection = new HubConnectionBuilder()
                .WithUrl(uri, options=>
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
            GetData(razgovor.Id);
        }
        private async void GetData(Guid id)
        {
            LoadCommand = new Command<object>(OnLoaded);
            var korisnikId = App.Korisnik.Id;
            var list = (await _apiServices.Poruke(id)).Select(p => new MessageInfo
            {
                DateTime = p.DatumIvrijeme.ToString("dd.MM.yyyy. HH:mm"),
                Text = p.Poruka1.Replace("<br />", "\n"),
                Username = p.IdKorisnikNavigation.Ime,
                TemplateType = p.IdKorisnik == korisnikId ? TemplateType.OutGoingText : TemplateType.IncomingText,
                ProfileImage = App.BackendUrl + p.IdKorisnikNavigation.GetLinkSlikaApi()

            });
            IsBusy = false;
            MessageInfo = new ObservableCollection<MessageInfo>(list);
            await Connect();
        }
        async Task Connect()
        {
            try
            {
                hubConnection.On<Poruka, string>("ReceiveMessageMob", (poruka, ime) =>
                {
                    if (poruka.IdRazgovor != razgovor.Id)
                        return;
                    var nova = new MessageInfo
                    {
                        DateTime = poruka.DatumIvrijeme.ToString(),
                        Text = poruka.Poruka1,
                        TemplateType = poruka.IdKorisnik == App.Korisnik.Id ? TemplateType.OutGoingText : TemplateType.IncomingText
                    };
                    var k = razgovor.KorisnikUrazgovoru.Where(k => k.IdKorisnik == poruka.IdKorisnik).SingleOrDefault();
                    nova.ProfileImage = App.BackendUrl + k.IdKorisnikNavigation.GetLinkSlikaApi();
                    nova.Username = k.IdKorisnikNavigation.Ime;
                    MessageInfo.Add(nova);
                    hubConnection.InvokeAsync("Procitano", razgovor.Id);
                });
                await hubConnection.StartAsync();
                if (hubConnection.State == HubConnectionState.Connected) ;
                    await hubConnection.InvokeAsync("Procitano", razgovor.Id);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

        }
        public async Task Disconnect()
        {
            await hubConnection.StopAsync();
        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void InitializeSendCommand()
        {
            SendIcon = "\ue745";
            SendCommand = new Command<object>(OnSendCommand);
            NewText = "";
        }

        private async void OnSendCommand(object obj)
        {
            var Listview = obj as Syncfusion.ListView.XForms.SfListView;
            if (!string.IsNullOrWhiteSpace(NewText))
            {
                await hubConnection.InvokeAsync("SendMessage", new
                {
                    IdRazg = razgovor.Id,
                    IdUser = App.Korisnik.Id,
                    Message = NewText,
                    When = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                }) ;

                /*MessageInfo.Add(new MessageInfo
                {
                    Text = NewText,
                    TemplateType = TemplateType.OutGoingText,
                    DateTime = string.Format("{0:HH:mm}", DateTime.Now)
                });*/
                (Listview.LayoutManager as LinearLayout).ScrollToRowIndex(MessageInfo.Count - 1, Syncfusion.ListView.XForms.ScrollToPosition.Start);
            }
            NewText = null;
        }

        private void OnLoaded(object obj)
        {
            var ListView = obj as Syncfusion.ListView.XForms.SfListView;
            var scrollView = ListView.Parent as ScrollView;
            ListView.HeightRequest = scrollView.Height;

            if (Device.RuntimePlatform == Device.macOS)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ListView.ScrollTo(2500);
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    (ListView.LayoutManager as LinearLayout).ScrollToRowIndex(this.MessageInfo.Count - 1, Syncfusion.ListView.XForms.ScrollToPosition.Start);
                });
            }
        }
    }
}
