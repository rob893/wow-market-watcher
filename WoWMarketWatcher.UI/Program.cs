using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using WoWMarketWatcher.UI.Services;
using Polly;
using Polly.Extensions.Http;
using WoWMarketWatcher.UI.Constants;
using WoWMarketWatcher.Common.Extensions;
using static WoWMarketWatcher.Common.Utilities.UtilityFunctions;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using WoWMarketWatcher.Common.Constants;
using System.Linq;
using System.Net;

namespace WoWMarketWatcher.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var services = builder.Services;

            services.AddMudServices();

            var unAuthPolicy = Policy.HandleResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.Unauthorized && response.Headers.TryGetValues(AppHeaderNames.TokenExpired, out var values) && values.Any())
            .RetryAsync(retryCount: 1, onRetryAsync: async (outcome, retryNumber, context) =>
            {
                var authService = services.BuildServiceProvider().GetRequiredService<IAuthService>();
                var res = await authService.RefreshTokenAsync();
                outcome.Result.RequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", res.Token);
            });

            services.AddHttpClient(HttpClientNames.AuthorizedWoWMarketWatcherAPI, client =>
            {
                var authService = services.BuildServiceProvider().GetRequiredService<IAuthService>();
                client.BaseAddress = new Uri(builder.Configuration[ConfigurationKeys.WoWMarketWatcherAPIBaseUrl]);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authService.AccessToken);
            })
            .AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(5, (retryAttempt) => TimeSpan.FromMilliseconds(retryAttempt * 300), onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var sourceName = GetSourceName();
                    var correlationId = outcome.Result.RequestMessage?.Headers.GetOrGenerateCorrelationId() ?? Guid.NewGuid().ToString();

                    services.BuildServiceProvider().GetRequiredService<ILogger<HttpClient>>()
                        .LogWarning(sourceName, correlationId, $"Request to {outcome.Result.RequestMessage?.RequestUri} failed with status {outcome.Result.StatusCode}. Delaying for {timespan.TotalMilliseconds}ms, then making retry {retryAttempt}.");
                })).AddPolicyHandler(unAuthPolicy);

            services.AddHttpClient(HttpClientNames.AnonymousWoWMarketWatcherAPI, client =>
            {
                var authService = services.BuildServiceProvider().GetRequiredService<IAuthService>();
                client.BaseAddress = new Uri(builder.Configuration[ConfigurationKeys.WoWMarketWatcherAPIBaseUrl]);
            }).AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(5, (retryAttempt) => TimeSpan.FromMilliseconds(retryAttempt * 300), onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var sourceName = GetSourceName();
                    var correlationId = outcome.Result.RequestMessage?.Headers.GetOrGenerateCorrelationId() ?? Guid.NewGuid().ToString();

                    services.BuildServiceProvider().GetRequiredService<ILogger<HttpClient>>()
                        .LogWarning(sourceName, correlationId, $"Request to {outcome.Result.RequestMessage?.RequestUri} failed with status {outcome.Result.StatusCode}. Delaying for {timespan.TotalMilliseconds}ms, then making retry {retryAttempt}.");
                }));

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<TestService>();
            services.AddBlazoredLocalStorage();

            await builder.Build().RunAsync();
        }
    }
}
