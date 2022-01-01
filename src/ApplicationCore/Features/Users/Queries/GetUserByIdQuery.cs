using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<Result<GetUserByIdQueryResponse>>
    {
        public string Id { get; set; }
    }

    internal class GetUserByIdQueryHandler :
        IRequestHandler<GetUserByIdQuery, Result<GetUserByIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly UserManager<AppUser> _userManager;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;

        public GetUserByIdQueryHandler(
            ILogger<GetUserByIdQueryHandler> logger,
            IUserSessionService userSession,
            UserManager<AppUser> userManager,
            AppDbContext dbContext,
            IStringLocalizer<GetUserByIdQueryHandler> localizer,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _userManager = userManager;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Result<GetUserByIdQueryResponse>> Handle(
            GetUserByIdQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _userManager.FindByIdAsync(request.Id);

                if (entity == null)
                {
                    return Result<GetUserByIdQueryResponse>.Fail(_localizer["User Not Found"]);
                }

                var roles = await _userManager.GetRolesAsync(entity);

                var dto = _mapper.Map<GetUserByIdQueryResponse>(entity);

                dto.Roles = roles;

                return Result<GetUserByIdQueryResponse>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {@0} {UserId}",
                   request, _userSession.UserId);
            }

            return Result<GetUserByIdQueryResponse>.Fail(_localizer["Internal Error"]);
        }
    }

    public class GetUserByIdQueryResponse
    {
        public string Id { get; set; }        
        public string ExternalId { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; } = "";

        public IEnumerable<string> Roles { get; set; }
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
