using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Roles.Commands
{
    public class DeleteRoleCommand : IRequest<Result<string>>
    {
        public string Id { get; set; }
    }

    internal class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;        
        readonly RoleManager<AppRole> _roleManager;

        public DeleteRoleCommandHandler(
            ILogger<DeleteRoleCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<DeleteRoleCommandHandler> localizer,            
            RoleManager<AppRole> roleManager)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;            
            _roleManager = roleManager;            
        }

        public async Task<Result<string>> Handle(
            DeleteRoleCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(request.Id);

                if (role == null)
                    return Result<string>.Fail(_localizer["Role Not Found!"]);

                // remove role from claims
                var claims = await _roleManager.GetClaimsAsync(role);

                if (claims != null && claims.Any())
                {
                    foreach (var claim in claims)
                    {
                        await _roleManager.RemoveClaimAsync(role, claim);
                    }
                }

                // delete role
                var result = await _roleManager.DeleteAsync(role);

                if (result.Succeeded)
                    return Result<string>.Success(_localizer["Role Deleted"]);
                else
                    _logger.LogError("Error deleting role {@0} {UserId}", result, _userSession.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {0} {UserId}",
                    request.Id, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer["Internal Error"]);
        }
    }
}
