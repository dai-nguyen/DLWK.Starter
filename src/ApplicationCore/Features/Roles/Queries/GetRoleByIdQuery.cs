using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Helpers;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Roles.Queries
{
    public class GetRoleByIdQuery : IRequest<Result<GetRoleByIdQueryResponse>>
    {
        public string Id { get; set; } = "";
    }

    internal class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<GetRoleByIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly RoleManager<AppRole> _roleManager;
        readonly IMemoryCache _cache;

        public GetRoleByIdQueryHandler(
            ILogger<GetRoleByIdQueryHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<GetRoleByIdQueryHandler> localizer,
            RoleManager<AppRole> roleManager,
            IMemoryCache cache)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _roleManager = roleManager;        
            _cache = cache;
        }

        public async Task<Result<GetRoleByIdQueryResponse>> Handle(
            GetRoleByIdQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var permission = _userSession.Claims.GetPermission(Const.ClaimNames.roles);

                if (!permission.can_read)
                    return Result<GetRoleByIdQueryResponse>.Fail(_localizer[Const.Messages.PermissionDenied]);

#pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(
                    $"GetRoleByIdQuery:{request.Id}",
                    async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                        var entity = await _roleManager.FindByIdAsync(request.Id);

                        if (entity == null)
                        {
                            return Result<GetRoleByIdQueryResponse>.Fail(_localizer["Role Not Found"]);
                        }

                        var claims = await _roleManager.GetClaimsAsync(entity);

                        var dto = new GetRoleByIdQueryResponse()
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            Description = entity.Description,
                            ExternalId = entity.ExternalId,
                        };

                        if (claims != null)
                        {
                            dto.Permissions = claims.ToRolePermissions();
                        }

                        return Result<GetRoleByIdQueryResponse>.Success(dto);
                    });
#pragma warning restore CS8603 // Possible null reference return.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role {@0} {UserId}",
                   request, _userSession.UserId);
            }
            return Result<GetRoleByIdQueryResponse>.Fail(_localizer[Const.Messages.InternalError]);
        }
    }

    public class GetRoleByIdQueryResponse
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public virtual string ExternalId { get; set; } = "";

        public IEnumerable<RolePermission> Permissions { get; set; }
            = Enumerable.Empty<RolePermission>();
    }
}
