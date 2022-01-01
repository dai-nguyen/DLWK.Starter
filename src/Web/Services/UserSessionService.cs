using ApplicationCore.Interfaces;
using System.Security.Claims;

namespace Web.Services
{
    public class UserSessionService : IUserSessionService
    {
        public UserSessionService(IHttpContextAccessor httpContextAccessor)
        {
            UserId = httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Claims = httpContextAccessor.HttpContext?.User?.Claims.AsEnumerable().Select(item => new KeyValuePair<string, string>(item.Type, item.Value)).ToArray();
        }

        public string UserId { get; }
        public IEnumerable<KeyValuePair<string, string>> Claims { get; set; }
    }
}
