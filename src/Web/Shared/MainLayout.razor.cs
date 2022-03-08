using ApplicationCore.States;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Shared
{
    public partial class MainLayout : IDisposable
    {
        [Inject]
        UserProfilePictureState _profilePictureState { get; set; }

        private MudBlazorAdminDashboard _theme = new();

        public bool _drawerOpen = true;

        void DrawerToggle()
        {            
            _drawerOpen = !_drawerOpen;
        }

        protected override void OnInitialized()
        {
            _profilePictureState.OnChange += StateHasChanged;

            StateHasChanged();
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
