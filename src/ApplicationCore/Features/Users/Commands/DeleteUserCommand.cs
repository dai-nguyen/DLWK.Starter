using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Commands
{
    public class DeleteUserCommand : IRequest<Result<string>>
    {
        public string Id { get; set; }
    }

    internal class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSession _userSession;
        readonly IStringLocalizer<DeleteUserCommandHandler> _localizer;
        readonly AppDbContext _dbContext;
        readonly UserManager<AppUser> _userManager;
        readonly IFileService _fileService;


        public DeleteUserCommandHandler(
            ILogger<DeleteUserCommandHandler> logger,
            IUserSession userSession,
            IStringLocalizer<DeleteUserCommandHandler> localizer,            
            AppDbContext dbContext,
            UserManager<AppUser> userManager,
            IFileService fileService)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _userManager = userManager;
            _fileService = fileService;
        }

        public async Task<Result<string>> Handle(
            DeleteUserCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(command.Id);

                if (user == null)
                    return Result<string>.Fail(_localizer["User Not Found!"]);

                // remove user from claims
                var claims = await _userManager.GetClaimsAsync(user);

                if (claims != null && claims.Any())
                {
                    await _userManager.RemoveClaimsAsync(user, claims);
                }

                // delete user
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                    return Result<string>.Success(_localizer["User Deleted"]);
                else
                    _logger.LogError("Error deleting user {@0} {UserId}", result, _userSession.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {0} {UserId}",
                    command.Id, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer["Internal Error"]);
        }
    }
}
