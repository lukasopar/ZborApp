using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ZborDataStandard.ViewModels.ZborViewModels;

namespace ZborMob.Services
{
    public class ApiServices
    {
        private HttpClient GetNewClient()
        {
            return new HttpClient
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
        }
        public async Task LoginAsync(string username, string password)
        {
            var keyValues = new 
            {
                Email = username,
                password = password
            };
            var _httpClient = GetNewClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "account/ApiLogin");
            request.Content = new StringContent( JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(content);
            // In case you need to send an auth token...
        }
        public async Task<IndexViewModel> PocetnaAsync()
        {
            var _httpClient = GetNewClient();
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Zborovi");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
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
            var _httpClient = GetNewClient();
            var request = new HttpRequestMessage(HttpMethod.Get, _httpClient.BaseAddress + "api/Obavijesti/" + id.ToString());
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
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
            var _httpClient = GetNewClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/lajkObavijesti");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            var response = await _httpClient.SendAsync(request);
            // In case you need to send an auth token...
        }
        public async Task UnLajkObavijestiAsync(Guid id)
        {
            var keyValues = new
            {
                IdCilj = id
            };
            var _httpClient = GetNewClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/unlajkObavijesti");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            var response = await _httpClient.SendAsync(request);
            // In case you need to send an auth token...
        }
        public async Task LajkKomentaraAsync(Guid id)
        {
            var keyValues = new
            {
                IdCilj = id
            };
            var _httpClient = GetNewClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/lajkKomentara");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            var response = await _httpClient.SendAsync(request);
            // In case you need to send an auth token...
        }
        public async Task UnLajkKomentaraAsync(Guid id)
        {
            var keyValues = new
            {
                IdCilj = id
            };
            var _httpClient = GetNewClient();
            var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "api/unlajkKomentara");
            request.Content = new StringContent(JsonConvert.SerializeObject(keyValues).ToString(), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            var response = await _httpClient.SendAsync(request);
            // In case you need to send an auth token...
        }
    }
}
