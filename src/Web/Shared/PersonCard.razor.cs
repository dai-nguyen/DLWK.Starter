using ApplicationCore.States;
using Microsoft.AspNetCore.Components;

namespace Web.Shared
{
    public partial class PersonCard : IDisposable
    {
        [Inject]
        UserProfilePictureState _profilePictureState { get; set; }

        protected override void OnInitialized()
        {
            _profilePictureState.OnChange += StateHasChanged;

            base.OnInitialized();
        }

        public void Dispose()
        {
            _profilePictureState.OnChange -= StateHasChanged;
        }
    }
}
