using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
    }
}
