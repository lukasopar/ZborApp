using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.KorisnikVIewModels;
using ZborDataStandard.ViewModels.PorukeViewModels;
using ZborDataStandard.ViewModels.RepozitorijViewModels;
using ZborDataStandard.ViewModels.ZborViewModels;
using ZborMob.Model;
using ZborMob.Views;

namespace ZborMob.Services
{
    public class ApiServices
    {
        private HttpClient _httpClient;
       
        public ApiServices()
        {
            _httpClient = new HttpClient
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

             );
            _httpClient.BaseAddress = new Uri($"{App.BackendUrl}/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            
        }
        private void PotrebanLogin()
        {
            string tokenFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "token.txt");
            File.WriteAllText(tokenFileName, "");
            App.Token = "";

            App.Current.MainPage = new LoginPage();
        }
        public async Task LoginAsync(string username, string password)
        {
            var keyValues = new 
            {
                Email = username,
                password = password
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/Token");
            request.Content = new StringContent( JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<Login>(content);

                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "korisnik.txt");
                File.WriteAllText(fileName, JsonConvert.SerializeObject(obj.Korisnik));
                App.Korisnik = obj.Korisnik;
                string tokenFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "token.txt");
                File.WriteAllText(tokenFileName, obj.Token);
                App.Token = obj.Token;

                App.Current.MainPage = new KorisnikMainPage();


            }

            // In case you need to send an auth token...
        }
        public async Task<IndexViewModel> PocetnaAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Zborovi");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var zborovi = JsonConvert.DeserializeObject<IndexViewModel>(content);
                return zborovi;
            }
            if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<ProfilViewModel> ProfilAsync(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Obavijesti/" + id.ToString());
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ProfilViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task LajkObavijestiAsync(Guid id)
        {
            var keyValues = new
            {
                IdCilj=id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/lajkObavijesti");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            // In case you need to send an auth token...
        }
        public async Task UnLajkObavijestiAsync(Guid id)
        {
            var keyValues = new
            {
                IdCilj = id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/unlajkObavijesti");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            // In case you need to send an auth token...
        }
        public async Task LajkKomentaraAsync(Guid id)
        {
            var keyValues = new
            {
                IdCilj = id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/lajkKomentara");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            // In case you need to send an auth token...
        }
        public async Task UnLajkKomentaraAsync(Guid id)
        {
            var keyValues = new
            {
                IdCilj = id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/unlajkKomentara");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            // In case you need to send an auth token...
        }
        public async Task<KomentarObavijesti> NoviKomentarAsync(string tekst, Guid id)
        {
            var keyValues = new
            {
                IdObavijest = id,
                Tekst = tekst

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/NoviKomentar/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<KomentarObavijesti>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<Obavijest> NovaObavijest(string naslov, string tekst, Guid id)
        {
            var keyValues = new ProfilViewModel
            {
                NovaObavijest = new Obavijest { Naslov = naslov, Tekst=tekst}

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/NovaObavijest/" + id);
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<Obavijest>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<PitanjaViewModel> PitanjaAsync(Guid id)
        {
            
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Pitanja/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<PitanjaViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task OdgovoriNaPitanje(Guid id, List<int> odgovori)
        {
            var keyValues = new
            {
                Id = id,
                Lista = odgovori

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/OdgovoriNaPitanje/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
              
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<Anketa> NovoPitanjeAsync(Guid id, Anketa pitanje)
        {
            
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/NovoPitanje/" + id);
            request.Content = new StringContent(JsonConvert.SerializeObject(pitanje).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<Anketa>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<AdministracijaViewModel> AdministracijaAsync(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Administracija/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<AdministracijaViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<ClanZbora> PrihvatiPrijavuZborAsync(PrijavaZaZbor prijava)
        {
            var keyValues = new
            {
                Value = prijava.Id,

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PrihvatiPrijavu/" );
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ClanZbora>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ObrisiPrijavuZborAsync(PrijavaZaZbor prijava)
        {
            var keyValues = new
            {
                Value = prijava.Id,

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/OdbijPrijavu/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task ObrisiPozivZborAsync(PozivZaZbor poziv)
        {
            var keyValues = new
            {
                Value = poziv.Id,

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiPoziv/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<List<Korisnik>> PretraziAsync(Guid id,string uvjet)
        {
            var keyValues = new
            {
                Id=id,
                Tekst = uvjet

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PretragaKorisnika/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<List<Korisnik>>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<PozivZaZbor> PozivZborAsync(Guid id, Guid idZbor)
        {
            var keyValues = new
            {
                Id = id,
                Naziv = idZbor

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PozivZaZbor/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<PozivZaZbor>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ObrisiClanZboraAsync(Guid id)
        {
            var keyValues = new AdministracijaViewModel
            {
                IdBrisanje = id

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiClana/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<ModeratorZbora> PostaviModeratoraAsync(Guid id, Guid idZbor)
        {
            var keyValues = new
            {
                IdKorisnik = id,
                IdCilj = idZbor

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/NoviModerator/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ModeratorZbora>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ObrisiModeratoraAsync(Guid id)
        {
            var keyValues = new
            {
                Value = id

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiModeratora/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task PostaviVoditeljaAsync(Guid id)
        {
            var keyValues = new AdministracijaViewModel
            {
                IdBrisanje = id

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PostaviVoditelja/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task PromjenaGlasaAsync(Guid id, int index)
        {
            var keyValues = new
            {
                Id = id,
                Poruka = index
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PromjenaGlasa/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<ProjektiMobViewModel> ProjektiAsync(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Projekti/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ProjektiMobViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task PrijavaProjektAsync(Guid id)
        {
            var keyValues = new
            {
                Id = id,
                Poruka = ""
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PrijavaZaProjekt/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task ObrisiPrijavuProjektAsync(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PovuciPrijavu/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<Projekt> NoviProjektAsync(Projekt model)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/Projekti/");
            request.Content = new StringContent(JsonConvert.SerializeObject(model).ToString(), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<Projekt>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<ProjektViewModel> ProjektAsync(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Projekt/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ProjektViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<ProjektViewModel> ObrisiDogadjaj(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiDogadjaj/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task NajavaDolaska(Guid id)
        {
            var keyvalues = new
            {
                IdCIlj = id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/NajavaDolaska/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyvalues).ToString(), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
               
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task UnNajavaDolaska(Guid id)
        {
            var keyvalues = new
            {
                IdCIlj = id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiNajavuDolaska/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyvalues).ToString(), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<List<VrstaDogadjaja>> VrsteDogadjajaAsync()
        {
            
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/VrsteDogadjaja/");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<List<VrstaDogadjaja>>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task DogadjajAsync(Guid id, Dogadjaj dog)
        {
            
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/NoviDogadjaj/" +id);
            request.Content = new StringContent(JsonConvert.SerializeObject(dog).ToString(), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<DogadjajViewModel> DogadjajViewModelAsync(Guid id)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Dogadjaj/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<DogadjajViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<AdministracijaProjektaViewModel> AdministracijaProjektaAsync(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/AdministracijaProjekta/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<AdministracijaProjektaViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<ClanNaProjektu> PrihvatiPrijavuProjektAsync(PrijavaZaProjekt prijava)
        {
            var keyValues = new
            {
                Value = prijava.Id,

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PrihvatiPrijavuProjekt/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ClanNaProjektu>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ObrisiPrijavuProjektAsync(PrijavaZaProjekt prijava)
        {
            var keyValues = new
            {
                Value = prijava.Id,

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/OdbijPrijavuProjekt/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
       
        public async Task<List<Korisnik>> PretraziProjektAsync(Guid id, string uvjet)
        {
            var keyValues = new
            {
                Id = id,
                Tekst = uvjet

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PretragaKorisnikaProjekt/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<List<Korisnik>>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<ClanNaProjektu> DodajClanProjektAsync(Guid idKorisnik, Guid idProjekt)
        {
            var keyValues = new
            {
                IdKorisnik = idKorisnik,
                IdCilj = idProjekt

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/DodajClanProjekt/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ClanNaProjektu>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ObrisiClanProjektaAsync(Guid id)
        {
            var keyValues = new AdministracijaViewModel
            {
                IdBrisanje = id

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiClanaProjekta/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task PromjenaUlogeAsync(ClanNaProjektu clan)
        {
            var keyValues = new
            {
                Id = clan.Id,
                Poruka = clan.Uloga
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PromjenaUloge/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<Statistika> DohvatiStatistikuAsync(Guid idKorisnik, Guid idProjekt)
        {
            var keyValues = new
            {
                IdKorisnik = idKorisnik,
                IdCilj = idProjekt

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/DohvatiStatistiku/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<Statistika>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ZavrsiProjektAsync(Guid id)
        {
            var keyValues = new AdministracijaViewModel
            {
                IdBrisanje = id

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ZavrsiProjekt/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task ObrisiProjektAsync(Guid id)
        {
            var keyValues = new AdministracijaViewModel
            {
                IdBrisanje = id

            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiProjekt/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<ZborDataStandard.ViewModels.ZborViewModels.JavniProfilViewModel> JavniProfilAsync(Guid idZbor)
        {
          
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/JavniProfil/" + idZbor);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ZborDataStandard.ViewModels.ZborViewModels.JavniProfilViewModel>(content);
                return obj;
            }
            return null;
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task SpremiOZboruAsync(Guid id, string tekst)
        {
            var keyValues = new
            {
                Id = id,
                Tekst = tekst
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/Urediozboru/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task SpremiOVoditeljimauAsync(Guid id, string tekst)
        {
            var keyValues = new
            {
                Id = id,
                Tekst = tekst
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/Urediovoditeljima/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task SpremiRepertoarAsync(Guid id, string tekst)
        {
            var keyValues = new
            {
                Id = id,
                Tekst = tekst
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/Uredirepertoar/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task SpremiReprezentacijauAsync(Guid id, string tekst)
        {
            var keyValues = new
            {
                Id = id,
                Tekst = tekst
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/Uredireprezentacija/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<RepozitorijZborViewModel> RepozitorijZborAsync(Guid idZbor)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/ZborRepozitorij/" + idZbor);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<RepozitorijZborViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ObrisiRepozitorijZborAsync(Guid id)
        {
            var keyValues = new RepozitorijZborViewModel
            {
                IdTrazeni = id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/DeleteZbor/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task VidljivostZborAsync(Guid id, string uri)
        {
            var keyValues = new
            {
                Value = id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/"+uri+"/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task PreuzmiZborAsync(RepozitorijZbor dat)
        {
            
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/GetRepozitorijZbor/" + dat.Id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                DependencyService.Get<IFileService>().Save(dat.Naziv, content);

            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<RepozitorijZbor> UploadZborAsync(Guid idZbor, string path, string name)
        {
            var file = File.ReadAllBytes(path);         
            var fileContent = new ByteArrayContent(file);

            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = name
            };

            string boundary = "---8d0f01e6b3b5dafaaadaad";
            MultipartFormDataContent multipartContent = new MultipartFormDataContent(boundary);
            multipartContent.Add(fileContent);
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/UploadRepozitorijZbor/" + idZbor);
            request.Content = multipartContent;
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<RepozitorijZbor>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<RepozitorijViewModel> RepozitorijKorisnikAsync(Guid idKorisnik)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/KorisnikRepozitorij/" + idKorisnik);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<RepozitorijViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ObrisiRepozitorijKorisnikAsync(Guid id)
        {
            var keyValues = new RepozitorijViewModel
            {
                IdTrazeni = id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiKorisnikRep/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task VidljivostKorisnikAsync(Guid id, string uri)
        {
            var keyValues = new
            {
                Value = id
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/" + uri + "/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task PreuzmiKorisnikAsync(RepozitorijKorisnik dat)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/GetKorisnikRep/" + dat.Id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                DependencyService.Get<IFileService>().Save(dat.Naziv, content);

            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<RepozitorijKorisnik> UploadKorisnikAsync(Guid idZbor, string path, string name)
        {
            var file = File.ReadAllBytes(path);
            var fileContent = new ByteArrayContent(file);

            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = name
            };

            string boundary = "---8d0f01e6b3b5dafaaadaad";
            MultipartFormDataContent multipartContent = new MultipartFormDataContent(boundary);
            multipartContent.Add(fileContent);
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/UploadRepozitorijZbor/" + idZbor);
            request.Content = multipartContent;
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<RepozitorijKorisnik>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<PorukeViewModel> Razgovori()
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/RazgovoriKorisnik/");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<PorukeViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<List<Poruka>> Poruke(Guid id)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Poruke/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<List<Poruka>>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<List<Korisnik>> KorisniciAsync()
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Korisnici/");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<List<Korisnik>>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<PretplateViewModel> PretplateAsync(Guid id)
        {
            
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Pretplate/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<PretplateViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<PretplateViewModel> PretplateSpremiAsync(Guid id, PretplateViewModel model)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/Pretplate/" + id);
            request.Content = new StringContent(JsonConvert.SerializeObject(model).ToString(), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<PretplateViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task EvidentirajAsync(DogadjajViewModel model)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/Evidentiraj/" );
            request.Content = new StringContent(JsonConvert.SerializeObject(model).ToString(), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<ObavijestiViewModel> Obavijesti()
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/ObavijestiOsobne/");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ObavijestiViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<ZborDataStandard.ViewModels.ZborViewModels.GalerijaViewModel> GalerijaZbor(Guid id)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/GalerijaZbor/" +id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ZborDataStandard.ViewModels.ZborViewModels.GalerijaViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ProfilnaZborAsync(Guid id)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PromjenaProfilneZbor/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<ZborDataStandard.ViewModels.KorisnikVIewModels.GalerijaViewModel> GalerijaKorisnik(Guid id)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/GalerijaKorisnik/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ZborDataStandard.ViewModels.KorisnikVIewModels.GalerijaViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task ProfilnaKorisnikAsync(Guid id)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PromjenaProfilneKorisnik/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task PrijavaZborAsync(Guid id)
        {

            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PrijavaZbor/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<ZborDataStandard.ViewModels.KorisnikVIewModels.JavniProfilViewModel> JavniProfilKorisnikAsync(Guid id)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/JavniProfilKorisnik/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ZborDataStandard.ViewModels.KorisnikVIewModels.JavniProfilViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task Spremiomeni( string tekst)
        {
            var keyValues = new
            {
                Tekst = tekst
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/Urediomeni/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task<ZborDataStandard.ViewModels.ForumViewModels.IndexViewModel> Forum()
        {
           
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/forum/");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ZborDataStandard.ViewModels.ForumViewModels.IndexViewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<ZborDataStandard.ViewModels.ForumViewModels.TemeViewModel> Teme(Guid id, int stranica = 1)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/tema/" + id +"?page="+stranica);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ZborDataStandard.ViewModels.ForumViewModels.TemeViewModel>(content);
                return obj;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task<ZborDataStandard.ViewModels.ForumViewModels.ZapisVIewModel> Zapisi(Guid id, int stranica = 1)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/zapis/" + id + "?page=" + stranica);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JsonConvert.DeserializeObject<ZborDataStandard.ViewModels.ForumViewModels.ZapisVIewModel>(content);
                return obj;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
            return null;
        }
        public async Task NoviZapis(Zapis novi)
        {
            var keyValues = new ZborDataStandard.ViewModels.ForumViewModels.ZapisVIewModel
            {
                Novi = novi
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/NoviZapis/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task UrediZapis(Guid idZapis, string tekst)
        {
            var keyValues = new 
            {
                Id = idZapis,
                Tekst = tekst
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/UrediZapis/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task ObrisiZapis(Guid idZapis)
        {
            var keyValues = new ZborDataStandard.ViewModels.ForumViewModels.ZapisVIewModel
            {
                IdBrisanje = idZapis
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiZapis/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task NovaTema(Tema nova, string tekst)
        {
            var keyValues = new ZborDataStandard.ViewModels.ForumViewModels.TemeViewModel
            {
                Nova = nova,
                Tekst = tekst,
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/NovaTema/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
        public async Task ObrisiTema(Guid idTema)
        {
            var keyValues = new ZborDataStandard.ViewModels.ForumViewModels.TemeViewModel
            {
                IdBrisanje = idTema
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiTema/");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
            }
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                PotrebanLogin();
            }
        }
    }
}
