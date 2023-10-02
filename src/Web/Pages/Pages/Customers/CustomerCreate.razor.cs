using ApplicationCore.Constants;
using ApplicationCore.Features.Customers.Commands;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Pages.Pages.Customers
{
    public partial class CustomerCreate
    {
        CreateCustomerCommand _command = new();

        List<string> Industries { get; set; } = new List<string>();

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        bool _canCreate = false;

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateTask;

            if (state.User.Identity.IsAuthenticated)
            {
                var permission = state.User.Claims.GetPermission(Const.ClaimNames.customers);

                if (permission != null)
                {
                    _canCreate = permission.can_create;
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
    }
}
