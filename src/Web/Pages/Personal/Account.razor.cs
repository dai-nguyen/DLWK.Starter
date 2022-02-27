using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Features.Users.Queries;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.Text.RegularExpressions;

namespace Web.Pages.Personal
{
    public partial class Account
    {
        public string AvatarImageLink { get; set; } = "https://avatars.githubusercontent.com/u/5643178?v=4"; //"images/avatar_jonny.jpg";
        public string AvatarIcon { get; set; }
        public string AvatarButtonText { get; set; } = "Delete Picture";
        public Color AvatarButtonColor { get; set; } = Color.Error;
        public string FirstName { get; set; } = "Dai";
        public string LastName { get; set; } = "Nguyen";
        public string JobTitle { get; set; } = "Application Developer";
        public string Email { get; set; } = "Youcanprobably@findout.com";
        public bool FriendSwitch { get; set; } = true;
        public bool NotificationEmail_1 { get; set; } = true;
        public bool NotificationEmail_2 { get; set; }
        public bool NotificationEmail_3 { get; set; }
        public bool NotificationEmail_4 { get; set; } = true;
        public bool NotificationChat_1 { get; set; }
        public bool NotificationChat_2 { get; set; } = true;
        public bool NotificationChat_3 { get; set; } = true;
        public bool NotificationChat_4 { get; set; }

        UpdateUserCommand _command = new();
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
                    _command.Id = user.Id;
                    _command.UserName = user.UserName;
                    _command.Email = user.Email;
                    _command.FirstName = user.FirstName;
                    _command.LastName = user.LastName;
                    _command.ExternalId = user.ExternalId;
                    _command.Roles = user.Roles;
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

        void SaveChanges(string message, Severity severity)
        {
            _snackBar.Add(message, severity, config =>
            {
                config.ShowCloseIcon = false;
            });
        }

        MudForm form;
        MudTextField<string> pwField1;

        private IEnumerable<string> PasswordStrength(string pw)
        {
            if (string.IsNullOrWhiteSpace(pw))
            {
                yield return "Password is required!";
                yield break;
            }
            if (pw.Length < 8)
                yield return "Password must be at least of length 8";
            if (!Regex.IsMatch(pw, @"[A-Z]"))
                yield return "Password must contain at least one capital letter";
            if (!Regex.IsMatch(pw, @"[a-z]"))
                yield return "Password must contain at least one lowercase letter";
            if (!Regex.IsMatch(pw, @"[0-9]"))
                yield return "Password must contain at least one digit";
        }

        private string PasswordMatch(string arg)
        {
            if (pwField1.Value != arg)
                return "Passwords don't match";
            return null;
        }
    }
}
