using Microsoft.AspNetCore.SignalR.Client;
using Syncfusion.DataSource.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.JSONModels;
using ZborMob.Services;

namespace ZborMob.ViewModels
{
    public class PorukeViewModel : INotifyPropertyChanged
    {
        private HubConnection hubConnection;
        private ApiServices _apiServices;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Razgovor> razgovori;
        public ObservableCollection<Razgovor> Razgovori
        {
            get
            {
                return razgovori;
            }
            set
            {
                razgovori = value;
                RaisepropertyChanged("Razgovori");
            }
        }
        private ZborDataStandard.ViewModels.PorukeViewModels.PorukeViewModel model;
        public PorukeViewModel()
        {
            IsBusy = true;
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
            GetData();
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
        private async void GetData()
        {
            model = await _apiServices.Razgovori();
            model.Razgovori.ForEach(r => r.Procitano = r.KorisnikUrazgovoru.Where(r => r.IdKorisnik == App.Korisnik.Id).SingleOrDefault().Procitano);
            IsBusy = false;
            Razgovori = new ObservableCollection<Razgovor>(model.Razgovori);
            await Connect();
        }
        async Task Connect()
        {
            try
            {
                hubConnection.On<Poruka, string>("ReceiveMessageMob", (poruka, ime) =>
                {
                    var razg = Razgovori.Where(razg => razg.Id == poruka.IdRazgovor).SingleOrDefault();
                    var list = new List<Poruka>();
                    list.Add(poruka);
                    if (poruka.IdKorisnik == App.Korisnik.Id)
                        razg.Procitano = true;
                    else
                        razg.Procitano = false;
                    razg.Poruka = list;
                    Razgovori.Remove(razg);
                    Razgovori.Insert(0, razg);
                    RaisepropertyChanged("Razgovori");
                });
                hubConnection.On<Razgovor, string>("ReceiveNewConversationMob", (razg, ime) =>
                {
                    var poruka = razg.Poruka.First();

                    if (poruka.IdKorisnik == App.Korisnik.Id)
                        razg.Procitano = true;
                    else
                        razg.Procitano = false;
                    Razgovori.Insert(0, razg);
                    RaisepropertyChanged("Razgovori");
                });
                hubConnection.On<Guid>("ProcitanaPoruka", (idRazg) =>
                {
                    var razg = Razgovori.Where(razg => razg.Id == idRazg).SingleOrDefault();
                    razg.Procitano = true;
                    var kor = razg.KorisnikUrazgovoru.Where(r => r.IdKorisnik == App.Korisnik.Id).SingleOrDefault().Procitano = true;
                    int index = Razgovori.IndexOf(razg);
                    Razgovori.Remove(razg);
                    Razgovori.Insert(index, razg);
                    RaisepropertyChanged("Razgovori");
                });
                await hubConnection.StartAsync();
                if (hubConnection.State == HubConnectionState.Connected)
                    await hubConnection.InvokeAsync("TestReq", "ovo je test");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

        }

        public async void Posalji(String tekst, List<string>kontakti)
        {
            await hubConnection.InvokeAsync("NewConversation", new
            {
                Kontakti = kontakti,
                IdUser = App.Korisnik.Id,
                Message = tekst,
                When = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            }); ; ;

        }
        async Task Disconnect()
        {
            await hubConnection.StopAsync();
        }
        void RaisepropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
