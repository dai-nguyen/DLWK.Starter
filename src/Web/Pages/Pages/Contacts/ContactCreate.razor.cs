using ApplicationCore.Constants;
using ApplicationCore.Features.Contacts.Commands;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Web.Pages.Pages.Contacts
{
    public partial class ContactCreate
    {
        CreateContactCommand _command = new();

        [CascadingParameter] MudDialogInstance MudDialog { get; set; }
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        bool _canEdit = false;
        bool _canDelete = false;

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateTask;

            if (state.User.Identity.IsAuthenticated)
            {
                var permission = state.User.Claims.GetPermission(Const.ClaimNames.contacts);

                if (permission != null)
                {
                    _canEdit = permission.can_edit;
                    _canDelete = permission.can_delete;
                }
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

                MudDialog.Close(DialogResult.Ok(true));
            }
        }

        void Cancel() => MudDialog.Cancel();
    }
}
