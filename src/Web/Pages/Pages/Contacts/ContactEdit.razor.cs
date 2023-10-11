using ApplicationCore.Constants;
using ApplicationCore.Features.Contacts.Commands;
using ApplicationCore.Features.Contacts.Queries;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Web.Pages.Pages.Contacts
{
    public partial class ContactEdit
    {
        [Parameter]
        public string id { get; set; }

        UpdateContactCommand _command = new();

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

            if (!string.IsNullOrEmpty(id))
            {
                var request = new GetContactByIdQuery() { Id = id };

                var res = await _mediator.Send(request);

                if (res.Succeeded)
                {
                    var data = res.Data;
                    _command.Id = data.Id;
                    _command.FirstName = data.FirstName;
                    _command.LastName = data.LastName;
                    _command.Email = data.Email;
                    _command.Phone = data.Phone;
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
