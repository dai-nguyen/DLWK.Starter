using ApplicationCore;
using ApplicationCore.Features.Roles.Commands;
using ApplicationCore.Models;

namespace Web.Pages.Pages.Roles
{
    public partial class RoleCreate
    {    
        CreateRoleCommand _command = new();
        
        IEnumerable<RolePermission> Permissions { get; set; } 
            = Constants.PermissionCheckList;

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
                if (result.Messages.Any())
                    _snackBar.Add(result.Messages.First(), MudBlazor.Severity.Success);

                _navigationManager.NavigateTo("/pages/roles");
            }
        }
    }
}
