using WoWMarketWatcher.UI.Models;
using System;

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WoWMarketWatcher.UI.Services
{
    public class AuthService
    {
        private readonly HttpClient httpClient;

        public AuthService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = new Uri("https://rherber-money-manager-service.azurewebsites.net/api/");
        }

        public async Task<LoginResponse> Login(string username, string password)
        {
            var response = await this.httpClient.PostAsJsonAsync("auth/login", new { username, password, deviceId = "123" });

            var asJson = await response.Content.ReadFromJsonAsync<LoginResponse>();

            return asJson;
        }
    }
}
