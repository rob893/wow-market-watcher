using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.Common.Models.Responses;
using WoWMarketWatcher.UI.Constants;

namespace WoWMarketWatcher.UI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ISyncLocalStorageService localStorageService;
        private readonly ILogger<AuthService> logger;
        private readonly JsonSerializerOptions defaultSerializerSettings = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };
        private const string accessTokenStorageKey = "access-token";

        private const string refreshTokenStorageKey = "refresh-token";

        private const string deviceIdStorageKey = "device-id";

        private const string userStorageKey = "user";

        private string cachedAccessToken;

        private string cachedRefreshToken;

        private string cachedDeviceId;

        private UserDto cachedLoggedInUser;

        public AuthService(IHttpClientFactory httpClientFactory, ISyncLocalStorageService localStorageService, ILogger<AuthService> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.localStorageService = localStorageService;
            this.logger = logger;
        }

        public bool IsUserLoggedIn => this.LoggedInUser != null;

        public UserDto LoggedInUser
        {
            get
            {
                if (this.cachedLoggedInUser == null)
                {
                    var userFromLocalStorage = this.localStorageService.GetItem<UserDto>(userStorageKey);

                    if (userFromLocalStorage == null)
                    {
                        return default;
                    }

                    this.cachedLoggedInUser = userFromLocalStorage;
                }

                return this.cachedLoggedInUser;
            }
        }

        public string AccessToken
        {
            get
            {
                if (this.cachedAccessToken == null)
                {
                    var tokenFromLocalStorage = this.localStorageService.GetItem<string>(accessTokenStorageKey);

                    if (tokenFromLocalStorage == null)
                    {
                        return default;
                    }

                    this.cachedAccessToken = tokenFromLocalStorage;
                }

                return this.cachedAccessToken;
            }
        }

        public string RefreshToken
        {
            get
            {
                if (this.cachedRefreshToken == null)
                {
                    var tokenFromLocalStorage = this.localStorageService.GetItem<string>(refreshTokenStorageKey);

                    if (tokenFromLocalStorage == null)
                    {
                        return default;
                    }

                    this.cachedRefreshToken = tokenFromLocalStorage;
                }

                return this.cachedRefreshToken;
            }
        }

        public string DeviceId
        {
            get
            {
                if (this.cachedDeviceId == null)
                {
                    var deviceIdFromLocalStorage = this.localStorageService.GetItem<string>(deviceIdStorageKey);

                    if (deviceIdFromLocalStorage == null)
                    {
                        this.cachedDeviceId = Guid.NewGuid().ToString();
                        this.localStorageService.SetItem(deviceIdStorageKey, this.cachedDeviceId);
                    }
                    else
                    {
                        this.cachedDeviceId = deviceIdFromLocalStorage;
                    }
                }

                return this.cachedDeviceId;
            }
        }

        public async Task<LoginResponse> Login(string username, string password)
        {
            var httpClient = this.httpClientFactory.CreateClient(HttpClientNames.AnonymousWoWMarketWatcherAPI);
            var response = await httpClient.PostAsJsonAsync("auth/login", new { username, password, deviceId = this.DeviceId });

            var asJson = await response.Content.ReadFromJsonAsync<LoginResponse>(this.defaultSerializerSettings);

            if (asJson == null)
            {
                throw new HttpRequestException("Login failed");
            }

            this.cachedAccessToken = asJson.Token;
            this.cachedRefreshToken = asJson.RefreshToken;
            this.cachedLoggedInUser = asJson.User;

            this.localStorageService.SetItem(accessTokenStorageKey, asJson.Token);
            this.localStorageService.SetItem(refreshTokenStorageKey, asJson.RefreshToken);
            this.localStorageService.SetItem(userStorageKey, asJson.User);

            return asJson;
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync()
        {
            var httpClient = this.httpClientFactory.CreateClient(HttpClientNames.AnonymousWoWMarketWatcherAPI);
            var response = await httpClient.PostAsJsonAsync("auth/refreshToken", new { token = this.AccessToken, refreshToken = this.RefreshToken, deviceId = this.DeviceId });

            var asJson = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>(this.defaultSerializerSettings);

            if (asJson == null)
            {
                throw new HttpRequestException("Login failed");
            }

            this.cachedAccessToken = asJson.Token;
            this.cachedRefreshToken = asJson.RefreshToken;

            this.localStorageService.SetItem(accessTokenStorageKey, asJson.Token);
            this.localStorageService.SetItem(refreshTokenStorageKey, asJson.RefreshToken);

            return asJson;
        }
    }
}
