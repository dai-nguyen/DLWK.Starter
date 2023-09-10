using ApplicationCore.Constants;
using ApplicationCore.States;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;

namespace Web.Shared
{
    public partial class MainLayout : IDisposable
    {
        [Inject]
        UserProfileState _profilePictureState { get; set; }
        [Inject]
        ProtectedLocalStorage _protectedLocalStore { get; set; }

        private MudBlazorAdminDashboard _theme = new();

        public bool _drawerOpen = true;

        public string AvatarIcon { get; set; } = Icons.Material.Outlined.Person;

        void DrawerToggle()
        {            
            _drawerOpen = !_drawerOpen;
        }

        protected override async Task OnInitializedAsync()
        {
            _profilePictureState.OnChange += StateHasChanged;

            if (string.IsNullOrEmpty(_profilePictureState.ProfilePicture))
            {
                var res = await _protectedLocalStore.GetAsync<string>(Const.LocalStorageKeys.ProfilePicture);

                if (res.Success)
                {
                    //_imageData = res.Value;
                    _profilePictureState.ProfilePicture = res.Value;
                }
            }

            //StateHasChanged();
        }

        public void Dispose()
        {
            _profilePictureState.OnChange -= StateHasChanged;
        }

        private List<BreadcrumbItem> _items = new List<BreadcrumbItem>
        {
            //new BreadcrumbItem("Personal", href: "#"),
            //new BreadcrumbItem("Dashboard", href: "#"),
        };
    }
}
