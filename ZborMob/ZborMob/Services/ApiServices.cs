using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ZborDataStandard.Model;
using ZborDataStandard.ViewModels.ZborViewModels;

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

        public async Task LoginAsync(string username, string password)
        {
            var keyValues = new 
            {
                Email = username,
                password = password
            };
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "account/ApiLogin");
            request.Content = new StringContent( JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(content);
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
        }
        public async Task NovoPitanjeAsync(Guid id, Anketa pitanje)
        {
            
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/NovoPitanje/" + id);
            request.Content = new StringContent(JsonConvert.SerializeObject(pitanje).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {

            }
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
        }
        public async Task ObrisiPrijavuProjektAsync(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/PovuciPrijavu/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
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
            return null;
        }
        public async Task<ProjektViewModel> ObrisiDogadjaj(Guid id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/ObrisiDogadjaj/" + id);
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
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
        }

    }
}
