using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Queries
{
    public class GetUserProfilePictureQuery : IRequest<Result<string>>
    {
        public string UserId { get; set; }
    }

    internal class GetUserProfilePictureQueryHandler : 
        IRequestHandler<GetUserProfilePictureQuery, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;        
        readonly AppDbContext _dbContext;        

        public GetUserProfilePictureQueryHandler(
            ILogger<GetUserProfilePictureQueryHandler> logger,
            IUserSessionService userSession,            
            AppDbContext dbContext,
            IStringLocalizer<GetUserProfilePictureQueryHandler> localizer)
        {
            _logger = logger;
            _userSession = userSession;
            _dbContext = dbContext;
            _localizer = localizer;
        }

        public async Task<Result<string>> Handle(
            GetUserProfilePictureQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var url = await _dbContext.Users
                    .Where(_ => _.Id == request.UserId)
                    .Select(_ => _.ProfilePictureUrl)
                    .FirstOrDefaultAsync();

                return Result<string>.Success(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {00} profile picture url {UserId}", 
                    request, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer["Internal Error"]);
        }

    }
}
