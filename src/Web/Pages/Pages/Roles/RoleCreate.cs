using ApplicationCore;
using ApplicationCore.Features.Roles.Commands;
using ApplicationCore.Models;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Pages.Pages.Roles
{
    public partial class RoleCreate
    {
        [Inject]
        ISnackbar _snackbar { get; set; }
        [Inject]
        IMediator _mediator { get; set; }

        CreateRoleCommand _command = new();

        List<string> Errors { get; set; } = new List<string>();

        IEnumerable<RolePermission> Permissions { get; set; } = Constants.PermissionCheckList;

        async Task SubmitAsync()
        {
            var result = await _mediator.Send(_command);

            if (!result.Succeeded)
            {
                Errors.AddRange(result.Messages);
            }
            else
            {
                _navigationManager.NavigateTo("/pages/roles");
            }
        }
    }
}
