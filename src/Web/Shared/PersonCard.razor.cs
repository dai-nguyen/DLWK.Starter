using ApplicationCore;
using ApplicationCore.States;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;

namespace Web.Shared
{
    public partial class PersonCard : IDisposable
    {
        [Inject]
        UserProfileState _profilePictureState { get; set; }
        [Inject]
        ProtectedLocalStorage _protectedLocalStore { get; set; }

        public string AvatarIcon { get; set; } = Icons.Material.Outlined.Person;

        [Parameter] public string Class { get; set; }
        [Parameter] public string Style { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _profilePictureState.OnChange += StateHasChanged;

            if (string.IsNullOrEmpty(_profilePictureState.ProfilePicture))
            {
                var res = await _protectedLocalStore.GetAsync<string>(Constants.LocalStorageKeys.ProfilePicture);

                if (res.Success)                                    
                    _profilePictureState.ProfilePicture = res.Value;                
            }

            if (string.IsNullOrEmpty(_profilePictureState.FullName))
            {
                var res = await _protectedLocalStore.GetAsync<string>(Constants.LocalStorageKeys.ProfileFullName);

                if (res.Success)
                    _profilePictureState.FullName = res.Value;
            }

            if (string.IsNullOrEmpty(_profilePictureState.Title))
            {
                var res = await _protectedLocalStore.GetAsync<string>(Constants.LocalStorageKeys.ProfileTitle);

                if (res.Success)
                    _profilePictureState.Title = res.Value;
            }

            await base.OnInitializedAsync();
        }

        public void Dispose()
        {
            _profilePictureState.OnChange -= StateHasChanged;
        }
    }
}
