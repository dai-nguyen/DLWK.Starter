using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Commands
{
    public class UpdateUserPictureCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = "";        
    }

    internal class UpdateUserPictureCommandHandler : IRequestHandler<UpdateUserPictureCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly UserManager<AppUser> _userManager;

        public UpdateUserPictureCommandHandler(
            ILogger<UpdateUserPictureCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateUserPictureCommandHandler> localizer,            
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;            
            _userManager = userManager;            
        }

        public async Task<Result<string>> Handle(
            UpdateUserPictureCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _userManager.FindByIdAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["User Not Found"]);
                }

                entity.ProfilePicture = command.ProfilePicture;

                var updated = await _userManager.UpdateAsync(entity);

                if (!updated.Succeeded)
                {
                    var errors = updated.Errors.Select(_ => _.Description).ToArray();
                    return Result<string>.Fail(errors);
                }

                return Result<string>.Success(_localizer["User Updated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating User {@0) {UserId}",
                    command, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer["Internal Error"]);
        }
    }

}
