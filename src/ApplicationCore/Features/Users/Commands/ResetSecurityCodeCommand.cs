using ApplicationCore.Data;
using ApplicationCore.Helpers;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Commands
{
    public class ResetSecurityCodeCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = string.Empty;        
    }

    internal class ResetSecurityCodeCommandHandler
        : IRequestHandler<ResetSecurityCodeCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly UserManager<AppUser> _userManager;

        public ResetSecurityCodeCommandHandler(
            ILogger<ChangePasswordCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<ResetSecurityCodeCommand> localizer,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(
            ResetSecurityCodeCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _userManager.FindByIdAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["User Not Found"]);
                }

                entity.SecurityCode = Helper.CreateRandomPasswordWithRandomLength();
                var res = await _userManager.UpdateAsync(entity);

                if (!res.Succeeded)
                {
                    var errors = res.Errors.Select(_ => _.Description).ToArray();
                    return Result<string>.Fail(errors);
                }

                return Result<string>.Success(entity.SecurityCode, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile user {@0} {UserId}",
                    command, _userSession.UserId);
            }
            return Result<string>.Fail();
        }
    }
}
