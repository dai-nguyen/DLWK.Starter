using ApplicationCore.Constants;
using ApplicationCore.Data;
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
    public class GetUserProfileByUserNameQuery : IRequest<Result<GetUserProfileByUserNameQueryResponse>>
    {
        public string UserName { get; set; } = string.Empty;
    }

    internal class GetUserProfileByUserNameQueryHandler :
        IRequestHandler<GetUserProfileByUserNameQuery, Result<GetUserProfileByUserNameQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly UserManager<AppUser> _userManager;        
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetUserProfileByUserNameQueryHandler(
            ILogger<GetUserProfileByUserNameQueryHandler> logger,
            IUserSessionService userSession,
            UserManager<AppUser> userManager,            
            IStringLocalizer<GetUserProfileByUserNameQueryHandler> localizer,
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

        public async Task<Result<GetUserProfileByUserNameQueryResponse>> Handle(
            GetUserProfileByUserNameQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userName = _userSession.UserName;

                if (userName != request.UserName)
                    return Result<GetUserProfileByUserNameQueryResponse>.Fail(Const.Messages.PermissionDenied);

#pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(
                    $"GetUserProfileByUserNameQuery:{request.UserName}",
                    async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                        var entity = await _userManager.FindByNameAsync(request.UserName);

                        if (entity == null)
                        {
                            return Result<GetUserProfileByUserNameQueryResponse>.Fail(_localizer["User Not Found"]);
                        }

                        var roles = await _userManager.GetRolesAsync(entity);

                        var dto = _mapper.Map<GetUserProfileByUserNameQueryResponse>(entity);

                        dto.Roles = roles;

                        return Result<GetUserProfileByUserNameQueryResponse>.Success(dto);
                    });
#pragma warning restore CS8603 // Possible null reference return.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {@0} {UserId}",
                   request, _userSession.UserId);
            }

            return Result<GetUserProfileByUserNameQueryResponse>.Fail(_localizer[Const.Messages.InternalError]);
        }
    }

    public class GetUserProfileByUserNameQueryResponse
    {
        public string Id { get; set; } = string.Empty;        

        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SecurityCode { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }

    public class GetUserByUserNameQueryProfile : Profile
    {
        public GetUserByUserNameQueryProfile()
        {
            CreateMap<AppUser, GetUserProfileByUserNameQueryResponse>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
        }
    }
}
