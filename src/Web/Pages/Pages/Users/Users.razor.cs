﻿using ApplicationCore.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Pages.Pages.Users
{
    public partial class Users
    {
        [Inject]
        ISnackbar _snackbar { get; set; }
        [Inject]
        IMediator _mediator { get; set; }

        int _total;
        string _searchString;
        IEnumerable<GetAllUsersQueryResponse> _pagedData;
        MudTable<GetAllUsersQueryResponse> _table;

        async Task<TableData<GetAllUsersQueryResponse>> ReloadData(TableState state)
        {
            var query = new GetAllUsersQuery(
                state.Page,
                state.PageSize,
                state.SortLabel,
                _searchString)
            {
                IsDescending = state.SortDirection == SortDirection.Descending
            };

            var result = await _mediator.Send(query);

            _total = result.TotalCount;

            return new TableData<GetAllUsersQueryResponse>()
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
