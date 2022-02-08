using ApplicationCore.Features.Roles.Commands;
using ApplicationCore.Features.Roles.Queries;
using Microsoft.AspNetCore.Components;

namespace Web.Pages.Pages.Roles
{
    public partial class RoleEdit
    {
        [Parameter] 
        public string id { get; set; }

        UpdateRoleCommand _command = new();

        protected override async Task OnInitializedAsync()
        {
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
    }
}
