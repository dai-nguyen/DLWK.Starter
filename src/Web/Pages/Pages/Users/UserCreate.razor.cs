using ApplicationCore.Features.Roles.Queries;
using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Pages.Pages.Users
{
    public partial class UserCreate
    {
        [Inject]
        ISnackbar _snackbar { get; set; }
        [Inject]
        IMediator _mediator { get; set; }

        CreateUserCommand _command = new();

        bool PasswordVisibility;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        List<string> Errors { get; set; } = new List<string>();

        List<GetAllRolesQueryResponse> Roles { get; set;} = new List<GetAllRolesQueryResponse>();

        protected override async Task OnInitializedAsync()
        {            
            var rolesRes = await _mediator.Send(new GetAllRolesQuery());

            if (rolesRes.Succeeded && rolesRes.Data.Any())
            {                
                Roles.AddRange(rolesRes.Data);
            }
        }

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
                _navigationManager.NavigateTo("/pages/users");
            }
        }
    }
}
