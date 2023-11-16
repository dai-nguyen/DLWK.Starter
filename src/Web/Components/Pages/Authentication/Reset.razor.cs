using ApplicationCore.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using Newtonsoft.Json;
using System.Text;

namespace Web.Components.Pages.Authentication
{
    public partial class Reset
    {
        [Inject]
        UserManager<AppUser> _userManager { get; set; }        

        [Parameter]
        public string Param { get; set; }        

        string Email { get; set; }
        string Password { get; set; }

        IList<string> Errors { get; set; } = new List<string>();

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

        async Task SubmitReset()
        {
            try
            {
                if (string.IsNullOrEmpty(Param))
                {
                    Errors.Add("Parameter is required.");
                    return;
                }

                if (string.IsNullOrEmpty(Email))
                {
                    Errors.Add("Email is required.");
                    return;
                }

                var user = await _userManager.FindByEmailAsync(Email);

                if (user == null)
                {
                    Errors.Add("Email not found.");
                    return;
                }

                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(Param));

                //var code = decoded.Replace('_', '/');

                var result = await _userManager.ResetPasswordAsync(user, decoded, Password);

                if (!result.Succeeded)
                {
                    Errors = result.Errors.Select(_ => _.Description).ToArray();
                }
                else
                {
                    _navigationManager.NavigateTo("/pages/authentication/login");
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
