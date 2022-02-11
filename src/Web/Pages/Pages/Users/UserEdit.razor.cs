﻿using ApplicationCore.Features.Roles.Queries;
using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Features.Users.Queries;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Web.Shared;

namespace Web.Pages.Pages.Users
{
    public partial class UserEdit
    {
        [Parameter]
        public string id { get; set; }

        UpdateUserCommand _command = new();

        bool PasswordVisibility;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        List<GetAllRolesQueryResponse> Roles { get; set; } = new List<GetAllRolesQueryResponse>();

        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrEmpty(id))
            {
                var request = new GetUserByIdQuery() { Id = id };

                var res = await _mediator.Send(request);

                if (res.Succeeded)
                {
                    var user = res.Data;
                    _command.Id = user.Id;
                    _command.UserName = user.UserName;
                    _command.Email = user.Email;
                    _command.FirstName = user.FirstName;
                    _command.LastName = user.LastName;
                    _command.ExternalId = user.ExternalId;
                    _command.Roles = user.Roles;
                }
            }

            var rolesRes = await _mediator.Send(new GetAllRolesQuery());

            if (rolesRes.Succeeded && rolesRes.Data.Any())
            {
                Roles.AddRange(rolesRes.Data);
            }
        }

        void TogglePasswordVisibility()
        {
            if (PasswordVisibility)
            {
                PasswordVisibility = false;
                PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                PasswordInput = InputType.Password;
            }
            else
            {
                PasswordVisibility = true;
                PasswordInputIcon = Icons.Material.Filled.Visibility;
                PasswordInput = InputType.Text;
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

                _navigationManager.NavigateTo("/pages/users");
            }
        }

        void GoBack()
        {
            _navigationManager.NavigateTo("/pages/users");
        }

        async Task Delete()
        {
            var p = new DialogParameters();
            p.Add("ContentText", $"Do you really want to delete user '{_command.UserName}'?");
            p.Add("ButtonText", "Delete");
            p.Add("Color", Color.Error);

            var options = new DialogOptions()
            {
                CloseButton = true,
                MaxWidth = MaxWidth.ExtraSmall
            };

            var dialog = _dialogService.Show<Dialog>("Delete", p, options);
            var result = await dialog.Result;

            if (!result.Cancelled)
            {
                var request = new DeleteUserCommand()
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

                    _navigationManager.NavigateTo("/pages/roles");
                }
            }
        }
    }
}
