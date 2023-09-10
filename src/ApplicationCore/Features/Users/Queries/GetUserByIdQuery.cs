using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Helpers;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<Result<GetUserByIdQueryResponse>>
    {
        public string Id { get; set; } = "";        
    }

    internal class GetUserByIdQueryHandler :
        IRequestHandler<GetUserByIdQuery, Result<GetUserByIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly UserManager<AppUser> _userManager;        
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetUserByIdQueryHandler(
            ILogger<GetUserByIdQueryHandler> logger,
            IUserSessionService userSession,
            UserManager<AppUser> userManager,            
            IStringLocalizer<GetUserByIdQueryHandler> localizer,
            IMapper mapper,
            IMemoryCache cache)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _userManager = userManager;            
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<Result<GetUserByIdQueryResponse>> Handle(
            GetUserByIdQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var permission = _userSession.Claims.GetPermission(Const.ClaimNames.users);

                if (!permission.can_read)
                    return Result<GetUserByIdQueryResponse>.Fail(_localizer[Const.Messages.PermissionDenied]);

#pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(
                    $"GetUserByIdQuery:{request.Id}",
                    async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                        AppUser entity = await _userManager.FindByIdAsync(request.Id);

                        if (entity == null)
                        {
                            return Result<GetUserByIdQueryResponse>.Fail(_localizer["User Not Found"]);
                        }

                        var roles = await _userManager.GetRolesAsync(entity);
                        var claims = await _userManager.GetClaimsAsync(entity);

                        var dto = _mapper.Map<GetUserByIdQueryResponse>(entity);

                        dto.Roles = roles;

                        if (claims != null)
                        {
                            dto.Claims = claims.Select(_ => new AppClaim(_.Type, _.Value))
                                .ToArray();
                        }

                        return Result<GetUserByIdQueryResponse>.Success(dto);
                    });
#pragma warning restore CS8603 // Possible null reference return.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {@0} {UserId}",
                   request, _userSession.UserId);
            }

            return Result<GetUserByIdQueryResponse>.Fail(_localizer[Const.Messages.InternalError]);
        }
    }

    public class GetUserByIdQueryResponse
    {
        public string Id { get; set; } = "";
        
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Title { get; set; } = "";
        public string ProfilePicture { get; set; } = "";
        
        public string ExternalId { get; set; } = "";

        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<AppClaim> Claims { get; set; }
            = Enumerable.Empty<AppClaim>();
    }

    public class GetUserByIdQueryProfile : Profile
    {
        public GetUserByIdQueryProfile()
        {
            CreateMap<AppUser, GetUserByIdQueryResponse>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
        }
    }
}
