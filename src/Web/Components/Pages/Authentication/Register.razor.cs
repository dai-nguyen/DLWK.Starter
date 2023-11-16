using ApplicationCore.Features.Users.Commands;
using MudBlazor;

namespace Web.Components.Pages.Authentication
{
    public partial class Register
    {        
        RegisterUserCommand _command = new();
                
        bool PasswordVisibility;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        List<string> Errors { get; set; } = new List<string>();

        void TogglePasswordVisibility()
        {
            if (PasswordVisibility)
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

        async Task SubmitAsync()
        {
            var result = await _mediator.Send(_command);

            if (!result.Succeeded)
            {
                Errors.AddRange(result.Messages);
            }
            else
            {                
                _navigationManager.NavigateTo("/pages/authentication/login");
            }
        }
    }
}
