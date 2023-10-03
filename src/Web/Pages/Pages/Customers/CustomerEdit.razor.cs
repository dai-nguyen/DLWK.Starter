using ApplicationCore.Constants;
using ApplicationCore.Features.Customers.Commands;
using ApplicationCore.Features.Customers.Queries;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Web.Shared;

namespace Web.Pages.Pages.Customers
{
    public partial class CustomerEdit
    {
        [Parameter]
        public string id { get; set; }

        UpdateCustomerCommand _command = new();

        List<string> Industries { get; set; } = new List<string>();

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        bool _canEdit = false;
        bool _canDelete = false;

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateTask;

            if (state.User.Identity.IsAuthenticated)
            {
                var permission = state.User.Claims.GetPermission(Const.ClaimNames.customers);

                if (permission != null)
                {
                    _canEdit = permission.can_edit;
                    _canDelete = permission.can_delete;
                }
            }

            if (!string.IsNullOrEmpty(id))
            {
                var request = new GetCustomerByIdQuery() { Id = id };

                var res = await _mediator.Send(request);

                if (res.Succeeded)
                {
                    var data = res.Data;
                    _command.Id = data.Id;
                    _command.Name = data.Name;
                    _command.Description = data.Description;
                    _command.Industries = data.Industries;
                    _command.Address1 = data.Address1;
                    _command.Address2 = data.Address2;
                    _command.City = data.City;
                    _command.State = data.State;
                    _command.Zip = data.Zip;
                    _command.Country = data.Country;
                }
            }

            Industries = new List<string> { "Aerospace", "Agricultural", "Automotive", "Basic metal", "Chemical" };
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

                _navigationManager.NavigateTo("/pages/customers");
            }
        }

        void GoBack()
        {
            _navigationManager.NavigateTo("/pages/customers");
        }

        async Task Delete()
        {
            var p = new DialogParameters
            {
                { "ContentText", $"Do you really want to delete '{_command.Name}'?" },
                { "ButtonText", "Delete" },
                { "Color", MudBlazor.Color.Error }
            };

            var options = new DialogOptions()
            {
                CloseButton = true,
                MaxWidth = MaxWidth.ExtraSmall
            };

            var dialog = _dialogService.Show<Dialog>("Delete", p, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var request = new DeleteCustomerCommand()
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

                    _navigationManager.NavigateTo("/pages/customers");
                }
            }
        }
    }
}
