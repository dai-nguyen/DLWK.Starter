using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.States;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using Web.Middleware;

namespace Web.Pages.Pages.Authentication
{
    public partial class Login : IDisposable
    {
        [Inject]
        ILogger<Login> _logger { get; set; }
        [Inject]
        UserManager<AppUser> _userManager { get; set; }
        [Inject]
        SignInManager<AppUser> _signinManager { get; set; }        
        [Inject]
        AuthenticationStateProvider _authenticationStateProvider { get; set; }
        [Inject]
        UserProfileState _profileState { get; set; }
        [Inject]
        ProtectedLocalStorage _protectedLocalStore { get; set; }

        string Username { get; set; }
        
        string Password { get; set; }
        bool RememberMe { get; set; }
        string ErrorMessage { get; set; } = "";

        bool PasswordVisibility;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;


        protected override async Task OnInitializedAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            _profileState.OnChange += StateHasChanged;

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
                if (string.IsNullOrEmpty(Username)
                    || string.IsNullOrWhiteSpace(Username)
                    || string.IsNullOrEmpty(Password)
                    || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Invalid Username and/or Password";
                    return;
                }

                var user = await _userManager.FindByNameAsync(Username);
                
                if (user == null)
                {
                    ErrorMessage = "Invalid Username and/or Password";
                    return;
                }

                var canSignIn = await _signinManager.CanSignInAsync(user);

                if (canSignIn)
                {
                    var result = await _signinManager.CheckPasswordSignInAsync(user, Password, true);

                    if (result == SignInResult.Success)
                    {
                        Guid key = Guid.NewGuid();
                        LoginMiddleware.Logins[key] = new LoginInfo 
                        { 
                            Username = Username, 
                            Password = Password,
                            RememberMe = RememberMe
                        };

                        var fullName = $"{user.FirstName} {user.LastName}";
                        
                        _profileState.ProfilePicture = user.ProfilePicture;
                        _profileState.FullName = fullName;
                        _profileState.Title = user.Title;

                        await _protectedLocalStore.SetAsync(Const.LocalStorageKeys.ProfilePicture, user.ProfilePicture);
                        await _protectedLocalStore.SetAsync(Const.LocalStorageKeys.ProfileFullName, fullName);
                        await _protectedLocalStore.SetAsync(Const.LocalStorageKeys.ProfileTitle, user.Title);

                        _navigationManager.NavigateTo($"/login?key={key}", true);                        
                    }
                    else
                        ErrorMessage = "Invalid Username and/or Password";
                }
                else
                {
                    ErrorMessage = "Invalid Username and/or Password";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Internal Server Error. Please try again later.";
                _logger.LogError(ex, "Error trying to login.");
            }

        }

        async Task Enter(string text)
        {
            Password = text;
            await SubmitLogin();
        }

        public void Dispose()
        {
            _profileState.OnChange -= StateHasChanged;
        }
    }
}
