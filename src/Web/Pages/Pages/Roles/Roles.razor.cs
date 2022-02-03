using ApplicationCore.Features.Roles.Queries;
using MudBlazor;

namespace Web.Pages.Pages.Roles
{
    public partial class Roles
    {
        int _total;
        string _searchString = string.Empty;
        MudTable<GetPaginatedRolesQueryResponse> _table;

        async Task<TableData<GetPaginatedRolesQueryResponse>> ReloadData(TableState state)
        {
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
    }
}
