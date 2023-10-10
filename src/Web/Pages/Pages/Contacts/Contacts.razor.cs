using ApplicationCore.Constants;
using ApplicationCore.Features.Contacts.Queries;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Web.Pages.Pages.Contacts
{
    public partial class Contacts
    {
        [Parameter]
        public string CustomerId { get; set; }

        int _total;
        string _searchString = string.Empty;
        MudTable<GetPaginatedContactsQueryResponse> _table;

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        bool _canCreate = false;

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateTask;

            if (state.User.Identity.IsAuthenticated)
            {
                var permission = state.User.Claims.GetPermission(Const.ClaimNames.contacts);

                if (permission != null)
                {
                    _canCreate = permission.can_create;
                }
            }

            await base.OnInitializedAsync();
        }

        async Task<TableData<GetPaginatedContactsQueryResponse>> ReloadData(TableState state)
        {
            var query = new GetPaginatedContactsQuery(
                state.Page,
                state.PageSize,
                state.SortLabel,
                CustomerId,
                _searchString,
                string.Empty)
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

            return new TableData<GetPaginatedContactsQueryResponse>()
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
            _navigationManager.NavigateTo("/pages/contacts/create");
        }

        void NavigateToEditPage(string id)
        {
            _navigationManager.NavigateTo($"/pages/contacts/edit/{id}");
        }
    }
}
