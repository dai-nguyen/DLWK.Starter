using ApplicationCore.Constants;
using ApplicationCore.Features.Roles.Queries;
using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Web.Pages.Pages.Users
{
    public partial class UserCreate
    {        

        CreateUserCommand _command = new();

        bool PasswordVisibility;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
        
        List<GetAllRolesQueryResponse> Roles { get; set;} = new List<GetAllRolesQueryResponse>();

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        bool _canCreate = false;

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateTask;

            if (state.User.Identity.IsAuthenticated)
            {
                var permission = state.User.Claims.GetPermission(Const.ClaimNames.users);

                if (permission != null)
                {
                    _canCreate = permission.can_create;
                }
            }

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
                foreach (var msg in result.Messages)
                {
                    _snackBar.Add(msg, MudBlazor.Severity.Error);
                }
            }
            else
            {
                foreach (var msg in result.Messages)
                {
                    _snackBar.Add(msg, MudBlazor.Severity.Success);
                }

                _navigationManager.NavigateTo("/pages/users");
            }
        }

        void GoBack()
        {
            _navigationManager.NavigateTo("/pages/users");
        }
    }
}
