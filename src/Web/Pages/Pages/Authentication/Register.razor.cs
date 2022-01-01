using ApplicationCore.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace Web.Pages.Pages.Authentication
{
    public partial class Register
    {
        [Inject]
        ILogger<Login> _logger { get; set; }
        [Inject]
        UserManager<AppUser> _userManager { get; set; }
        [Inject]
        NavigationManager _navigationManager { get; set; }

        string Password { get; set; }
        public bool AgreeToTerms { get; set; }

        bool PasswordVisibility;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

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

        async Task SubmitRegister()
        {

        }
    }
}
