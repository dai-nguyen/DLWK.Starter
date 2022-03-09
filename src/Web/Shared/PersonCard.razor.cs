using ApplicationCore;
using ApplicationCore.States;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Web.Shared
{
    public partial class PersonCard : IDisposable
    {
        [Inject]
        UserProfilePictureState _profilePictureState { get; set; }
        [Inject]
        ProtectedLocalStorage _protectedLocalStore { get; set; }

        protected override async Task OnInitializedAsync()
        {
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

            base.OnInitialized();
        }

        public void Dispose()
        {
            _profilePictureState.OnChange -= StateHasChanged;
        }
    }
}
