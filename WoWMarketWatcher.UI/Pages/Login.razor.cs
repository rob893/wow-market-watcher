using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
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
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private ILogger<Login> Logger { get; set; }

        private string Username { get; set; }

        private string Password { get; set; }

        private MudForm LoginForm { get; set; }

        private bool passwordVisibility;

        private InputType passwordInput = InputType.Password;

        private string passwordInputIcon = Icons.Material.Filled.VisibilityOff;

        private void TogglePasswordVisibility()
        {
            if (this.passwordVisibility)
            {
                this.passwordVisibility = false;
                this.passwordInputIcon = Icons.Material.Filled.VisibilityOff;
                this.passwordInput = InputType.Password;
            }
            else
            {
                this.passwordVisibility = true;
                this.passwordInputIcon = Icons.Material.Filled.Visibility;
                this.passwordInput = InputType.Text;
            }
        }

        private async Task LoginAsync()
        {
            this.Logger.LogInformation("Logging in!");

            var res = await this.AuthService.Login(this.Username, this.Password);

            this.Logger.LogInformation(res.User.UserName);

            this.NavigationManager.NavigateTo("watchLists");
        }

        private async Task ClearLoginForm()
        {
            await this.TestService.Test();
            this.Username = "";
            this.Password = "";
        }
    }
}