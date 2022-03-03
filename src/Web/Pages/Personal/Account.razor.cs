using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Features.Users.Queries;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Web.Pages.Personal
{
    public partial class Account
    {
        public string AvatarImageLink { get; set; } = "https://avatars.githubusercontent.com/u/5643178?v=4"; //"images/avatar_jonny.jpg";
        public string AvatarIcon { get; set; }
        public string AvatarButtonText { get; set; } = "Delete Picture";
        public Color AvatarButtonColor { get; set; } = Color.Error;
        

        UpdateUserProfileCommand _profileCommand = new();
        ChangePasswordCommand _changePasswordCommand = new();

        GetUserProfileByUserNameQueryResponse _profile = new();

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        bool _canEdit = false;
        bool _canDelete = false;

        protected override async Task OnInitializedAsync()
        {
            var state = await authenticationStateTask;

            if (state.User.Identity.IsAuthenticated)
            {
                var request = new GetUserProfileByUserNameQuery()
                {
                    Name = state.User.Identity.Name
                };

                var res = await _mediator.Send(request);

                if (res.Succeeded)
                {
                    _profile = res.Data;

                    var user = res.Data;
                    _profileCommand.Id = user.Id;
                    _profileCommand.Email = user.Email;
                    _profileCommand.FirstName = user.FirstName;
                    _profileCommand.LastName = user.LastName;                    

                    _changePasswordCommand.Id = user.Id;
                }
            }

            await base.OnInitializedAsync();
        }

        void DeletePicture()
        {
            if (!String.IsNullOrEmpty(AvatarImageLink))
            {
                AvatarImageLink = null;
                AvatarIcon = Icons.Material.Outlined.SentimentVeryDissatisfied;
                AvatarButtonText = "Upload Picture";
                AvatarButtonColor = Color.Primary;
            }
            else
            {
                return;
            }
        }

        async Task SaveProfileAsync()
        {
            var result = await _mediator.Send(_profileCommand);

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
            }
        }

        async Task ChangePasswordAsync()
        {
            var result = await _mediator.Send(_changePasswordCommand);

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
            }
        }
    }
}
