using ApplicationCore.Models;
using System.Security.Claims;

namespace ApplicationCore.Interfaces
{
    public interface IUserSessionService
    {
        string UserId { get; }
        string UserName { get; }
        IEnumerable<Claim> Claims { get; }
    }
}
