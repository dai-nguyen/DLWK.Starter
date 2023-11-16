using ApplicationCore.Constants;
using ApplicationCore.Features.Contacts.Commands;
using ApplicationCore.Features.Contacts.Queries;
using ApplicationCore.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Web.Components.Pages.Contacts
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

        async Task OpenCreateDialogAsync()
        {
            var dialogOptions = new DialogOptions()
            {
                FullScreen = true,
                CloseButton = true,
            };

            await _dialogService.ShowAsync<ContactCreate>("Create Contact", dialogOptions);
            await _table.ReloadServerData();
        }

        async Task OpenEditDialog(string id)
        {
            var dialogOptions = new DialogOptions()
            {
                FullScreen = true,
                CloseButton = true,
            };

            var parameters = new DialogParameters<string>
            {
                { "id", id }
            };

            await _dialogService.ShowAsync<ContactEdit>("Edit Contact", parameters, dialogOptions);
            await _table.ReloadServerData();
        }

        async Task Delete(GetPaginatedContactsQueryResponse model)
        {
            var dialogOptions = new DialogOptions()
            {
                CloseButton = true,
                CloseOnEscapeKey = true,
            };

            var msgBoxOption = new MessageBoxOptions()
            {
                Title = "Are you sure?",
                Message = $"Are you sure you want to delete '{model.FirstName} {model.LastName}'?",
                YesText = "Yes",
                NoText = "No"
            };

            var result = await _dialogService.ShowMessageBox(msgBoxOption, dialogOptions);

            if (result.HasValue && result.Value) 
            {
                var deleteCmd = new DeleteContactCommand()
                {
                    Id = model.Id
                };

                var res = await _mediator.Send(deleteCmd);

                if (!res.Succeeded)
                {
                    foreach (var msg in res.Messages)
                    {
                        _snackBar.Add(msg, MudBlazor.Severity.Error);
                    }
                }
                else
                {
                    foreach (var msg in res.Messages)
                    {
                        _snackBar.Add(msg, MudBlazor.Severity.Success);
                    }                    
                    await _table.ReloadServerData();
                }
            }
        }
    }
}
