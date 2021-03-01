using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using WoWMarketWatcher.UI.Services;

namespace WoWMarketWatcher.UI.Pages
{
    public partial class Login
    {
        [Inject]
        private IAuthService AuthService { get; set; }

        [Inject]
        private TestService TestService { get; set; }

        [Inject]
        private ILogger<Login> Logger { get; set; }

        private string Username { get; set; }

        private string Password { get; set; }

        public async Task LoginAsync()
        {
            this.Logger.LogInformation("Logging in!");

            var res = await this.AuthService.Login(this.Username, this.Password);

            this.Logger.LogInformation(res.User.UserName);
        }

        public async Task ClearLoginForm()
        {
            await this.TestService.Test();
            this.Username = "";
            this.Password = "";
        }
    }
}
