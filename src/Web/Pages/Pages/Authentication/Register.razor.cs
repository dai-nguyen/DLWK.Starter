using ApplicationCore.Features.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Pages.Pages.Authentication
{
    public partial class Register
    {
        [Inject]
        ILogger<Login> _logger { get; set; }
        [Inject]
        IMediator _mediator { get; set; }

        RegisterUserCommand _command = new();
        
        

        bool PasswordVisibility;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        List<string> Errors { get; set; } = new List<string>();

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
