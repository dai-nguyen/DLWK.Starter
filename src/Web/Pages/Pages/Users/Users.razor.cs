using ApplicationCore;
using ApplicationCore.Features.Users.Queries;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Web.Pages.Pages.Users
{
    public partial class Users
    {
        int _total;
        string _searchString = string.Empty;        
        MudTable<GetPaginatedUsersQueryResponse> _table;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        bool _canCreate = false;        

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateTask;

            if (state.User.Identity.IsAuthenticated)
            {
                var permission = state.User.Claims.GetPermission(Constants.ClaimNames.users);

                if (permission != null)
                {
                    _canCreate = permission.can_create;                    
                }
            }

            await base.OnInitializedAsync();
        }

        async Task<TableData<GetPaginatedUsersQueryResponse>> ReloadData(TableState state)
        {
            var query = new GetPaginatedUsersQuery(
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

            return new TableData<GetPaginatedUsersQueryResponse>()
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
            _navigationManager.NavigateTo("/pages/users/create");
        }

        void NavigateToEditPage(string id)
        {
            _navigationManager.NavigateTo($"/pages/users/edit/{id}");
        }
    }
}
