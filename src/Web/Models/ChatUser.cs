using MudBlazor;

namespace Web.Models
{
    public class ChatUser
    {
        public string UserName { get; set; }
        public string UserRoleColor { get; set; }
        public MudBlazor.Color OnlineStatus { get; set; }
        public bool Spotify { get; set; }
        public string AvatarUrl { get; set; }
        public MudBlazor.Color AvatarColor { get; set; }
    }
}
