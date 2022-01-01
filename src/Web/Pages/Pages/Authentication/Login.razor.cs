using ApplicationCore.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using System.Security.Claims;
using Web.Middleware;

namespace Web.Pages.Pages.Authentication
{
    public partial class Login
    {
        [Inject]
        ILogger<Login> _logger { get; set; }
        [Inject]
        UserManager<AppUser> _userManager { get; set; }
        [Inject]
        SignInManager<AppUser> _signinManager { get; set; }
        [Inject]
        NavigationManager _navigationManager { get; set; }
        [Inject]
        AuthenticationStateProvider authenticationStateProvider { get; set; }

        string Username { get; set; }
        string Password { get; set; }
        bool RememberMe { get; set; }

        bool PasswordVisibility;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;


        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity.IsAuthenticated)
            {
                _navigationManager.NavigateTo("/personal/dashboard");
            }
        }

        void TogglePasswordVisibility()
        {
            if(PasswordVisibility)
            {
                PasswordVisibility = false;
                PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                PasswordInput = InputType.Password;
            }
            else
            {
                PasswordVisibility = true;
                PasswordInputIcon = Icons.Material.Filled.Visibility;
                PasswordInput = InputType.Text;
            }
        }

        //https://github.com/dotnet/aspnetcore/issues/13601
        private async Task SubmitLogin()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(Username);
                
                if (user == null)
                {
                    return;
                }

                var canSignIn = await _signinManager.CanSignInAsync(user);

                if (canSignIn)
                {
                    var result = await _signinManager.CheckPasswordSignInAsync(user, Password, true);

                    if (result == Microsoft.AspNetCore.Identity.SignInResult.Success)
                    {
                        Guid key = Guid.NewGuid();
                        BlazorCookieLoginMiddleware.Logins[key] = new LoginInfo { Username = Username, Password = Password };
                        _navigationManager.NavigateTo($"/login?key={key}", true);                        
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to login.");
            }

        }
    }
}
