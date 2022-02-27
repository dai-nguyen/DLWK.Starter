using ApplicationCore;
using ApplicationCore.Features.Roles.Commands;
using ApplicationCore.Helpers;
using ApplicationCore.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Pages.Pages.Roles
{
    public partial class RoleCreate
    {    
        CreateRoleCommand _command = new();
        
        IEnumerable<RolePermission> Permissions { get; set; } 
            = Constants.PermissionCheckList;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        bool _canCreate = false;

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateTask;

            if (state.User.Identity.IsAuthenticated)
            {
                var permission = state.User.Claims.GetPermission(Constants.ClaimNames.roles);

                if (permission != null)
                {
                    _canCreate = permission.can_create;                    
                }
            }

            await base.OnInitializedAsync();
        }

        async Task SubmitAsync()
        {
            _command.Permissions = Permissions
                .Where(_ => _.can_read || _.can_edit || _.can_create || _.can_delete)
                .ToArray();
            
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
                
                _navigationManager.NavigateTo("/pages/roles");
            }
        }

        void GoBack()
        {
            _navigationManager.NavigateTo("/pages/roles");
        }
    }
}
