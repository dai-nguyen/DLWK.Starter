using ApplicationCore.Features.Roles.Queries;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Web.Pages.Pages.Roles
{
    public partial class Roles
    {
        int _total;
        string _searchString = string.Empty;
        MudTable<GetPaginatedRolesQueryResponse> _table;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        async Task<TableData<GetPaginatedRolesQueryResponse>> ReloadData(TableState state)
        {
            var authState = await authenticationStateTask;
            var user = authState.User;
            var clams = user.Claims;

            var query = new GetPaginatedRolesQuery(
                state.Page,
                state.PageSize,
                state.SortLabel,
                _searchString)
            {
                IsDescending = state.SortDirection == SortDirection.Descending,
            };

            var result = await _mediator.Send(query);

            if (!result.Succeeded && result.Messages != null && result.Messages.Any())
            {
                foreach (var message in result.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }

            _total = result.Data.Count();

            return new TableData<GetPaginatedRolesQueryResponse>()
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
            _navigationManager.NavigateTo("/pages/roles/create");
        }

        void NavigateToEditPage(string id)
        {
            _navigationManager.NavigateTo($"/pages/roles/edit/{id}");
        }
    }
}
