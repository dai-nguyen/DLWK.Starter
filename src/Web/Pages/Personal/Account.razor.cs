using ApplicationCore;
using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Features.Users.Queries;
using ApplicationCore.States;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Web.Pages.Personal
{
    public partial class Account : IDisposable
    {
        [Inject]
        UserProfilePictureState _profilePictureState { get; set; }
        [Inject]
        ProtectedLocalStorage _protectedLocalStore { get; set; }
        
        public string AvatarIcon { get; set; }
        public string AvatarButtonText { get; set; } = "Delete Picture";
        public MudBlazor.Color AvatarButtonColor { get; set; } = MudBlazor.Color.Error;
        

        UpdateUserProfileCommand _profileCommand = new();
        ChangePasswordCommand _changePasswordCommand = new();

        GetUserProfileByUserNameQueryResponse _profile = new();
        
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        
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
                    _profileCommand.Title = user.Title;

                    _changePasswordCommand.Id = user.Id;
                }
            }

            _profilePictureState.OnChange += StateHasChanged;

            if (string.IsNullOrEmpty(_profilePictureState.ProfilePicture))
            {
                var res = await _protectedLocalStore.GetAsync<string>(Constants.LocalStorageKeys.ProfilePicture);

                if (res.Success)
                {
                    //_imageData = res.Value;
                    _profilePictureState.ProfilePicture = res.Value;
                }
            }

            await base.OnInitializedAsync();
        }

        void DeletePicture()
        {
            //_imageData = string.Empty;
            _profilePictureState.ProfilePicture = String.Empty;

            //if (!String.IsNullOrEmpty(AvatarImageLink))
            //{
            //    AvatarImageLink = null;
            //    AvatarIcon = Icons.Material.Outlined.Person; //SentimentVeryDissatisfied;
            //    //AvatarButtonText = "Upload Picture";
            //    //AvatarButtonColor = Color.Primary;
            //}
            //else
            //{
                
            //}
        }

        private async void UploadImage(InputFileChangeEventArgs e)
        {
            var file = e.File;
            //long size = file.Size;
            await using MemoryStream ms = new();
            await file.OpenReadStream().CopyToAsync(ms);            
            var bytes = ms.ToArray();
            
            await using MemoryStream ms2 = new();
            using (Image img = Image.Load(bytes))
            {
                using (Image dest = img.Clone(_ => _.Resize(new ResizeOptions()
                {
                    Size = new SixLabors.ImageSharp.Size(200),
                    Mode = ResizeMode.Crop
                })))
                {
                    await dest.SaveAsJpegAsync(ms2);
                }
            }

            var bytes2 = ms2.ToArray();

            var str = Convert.ToBase64String(bytes2);            
            var data = $"data:image/jpeg;base64,{str}";
                        
            var request = new UpdateUserPictureCommand()
            {
                Id = _profile.Id,
                ProfilePicture = data
            };

            var res = await _mediator.Send(request);

            if (res.Succeeded)
            {
                await _protectedLocalStore.SetAsync(Constants.LocalStorageKeys.ProfilePicture, data);
                _profilePictureState.ProfilePicture = data;                                
            }

            // "data:image/png;base64,{str}"
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

        public void Dispose()
        {
            _profilePictureState.OnChange -= StateHasChanged;
        }
    }
}
