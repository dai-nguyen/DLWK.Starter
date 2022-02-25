using ApplicationCore.Features.Roles.Commands;
using ApplicationCore.Features.Roles.Queries;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Web.Shared;

namespace Web.Pages.Pages.Roles
{
    public partial class RoleEdit
    {
        [Parameter] 
        public string id { get; set; }

        UpdateRoleCommand _command = new();

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        bool _canEdit = false;
        bool _canDelete = false;

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateTask;

            if (state.User.Identity.IsAuthenticated)
            {
                var permission = state.User.Claims.GetPermission(Constants.ClaimNames.roles);

                if (permission != null)
                {                    
                    _canEdit = permission.can_edit;
                    _canDelete = permission.can_delete;
                }
            }

            if (!string.IsNullOrEmpty(id))
            {
                var request = new GetRoleByIdQuery() { Id = id };

                var res = await _mediator.Send(request);

                if (res.Succeeded)
                {
                    var role = res.Data;
                    _command.Id = id;
                    _command.Name = role.Name;
                    _command.Description = role.Description;
                    _command.ExternalId = role.ExternalId;
                    _command.Permissions = role.Permissions;
                }
            }

            await base.OnInitializedAsync();
        }

        async Task SubmitAsync()
        {
            _command.Permissions = _command.Permissions
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

        void GoBack()
        {
            _navigationManager.NavigateTo("/pages/roles");
        }

        async Task Delete()
        {
            var p = new DialogParameters();
            p.Add("ContentText", $"Do you really want to delete role '{_command.Name}'?");
            p.Add("ButtonText", "Delete");
            p.Add("Color", Color.Error);

            var options = new DialogOptions()
            {
                CloseButton = true,
                MaxWidth = MaxWidth.ExtraSmall
            };

            var dialog = _dialogService.Show<Dialog>("Delete", p, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var request = new DeleteRoleCommand()
                {
                    Id = _command.Id
                };

                var res = await _mediator.Send(request);

                if (!res.Succeeded)
                {
                    foreach (var msg in res.Messages)
                    {
                        _snackBar.Add(msg, MudBlazor.Severity.Error);
                    }
                }
                else
                {
                    if (res.Messages.Any())
                        _snackBar.Add(res.Messages.First(), MudBlazor.Severity.Success);

                    _navigationManager.NavigateTo("/pages/roles");
                }
            }
        }

    }
}
