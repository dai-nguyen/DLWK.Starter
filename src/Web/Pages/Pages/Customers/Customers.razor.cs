using ApplicationCore.Constants;
using ApplicationCore.Features.Customers.Queries;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Web.Pages.Pages.Customers
{
    public partial class Customers
    {
        int _total;
        string _searchString = string.Empty;
        MudTable<GetPaginatedCustomersQueryResponse> _table;

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

            await base.OnInitializedAsync();
        }

        async Task<TableData<GetPaginatedCustomersQueryResponse>> ReloadData(TableState state)
        {
            var query = new GetPaginatedCustomersQuery(
                state.Page,
                state.PageSize,
                state.SortLabel,
                _searchString)
            {
                IsDescending = state.SortDirection == SortDirection.Descending
            };

            var result = await _mediator.Send(query);

            if (!result.Succeeded && result.Messages != null && result.Messages.Any())
            {
                foreach (var message in result.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }

            _total = result.TotalCount;

            return new TableData<GetPaginatedCustomersQueryResponse>()
            {
                TotalItems = _total,
                Items = result.Data
            };
        }

        async Task OnSearch(string text)
        {
            _searchString = text;
            await _table.ReloadServerData();
        }

        void NagivateToCreatePage()
        {
            _navigationManager.NavigateTo("/pages/customers/create");
        }

        void NavigateToEditPage(string id)
        {
            _navigationManager.NavigateTo($"/pages/customers/edit/{id}");
        }
    }
}
