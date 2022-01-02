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

        string FirstName { get; set; }
        string LastName { get; set; }
        string Username { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        public bool AgreeToTerms { get; set; }

        bool PasswordVisibility;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        IList<string> Errors { get; set; } = new List<string>();

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
            // check username
            var found = await _userManager.FindByNameAsync(Username);

            if (found != null)
            {
                Errors.Add("Username is already used.");
                return;
            }

            // check email
            found = await _userManager.FindByEmailAsync(Email);

            if (found != null)
            {
                Errors.Add("Email is already used.");
                return;
            }

            var entity = new AppUser()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = FirstName,
                LastName = LastName,
                UserName = Username,
                Email = Email,
            };

            var result = await _userManager.CreateAsync(entity, Password);

            if (!result.Succeeded)
            {
                Errors = result.Errors.Select(_ => _.Description).ToArray();
            }
            else
            {
                await _userManager.AddToRoleAsync(entity, "User");
                _navigationManager.NavigateTo("/pages/authentication/login");
            }
        }
    }
}
