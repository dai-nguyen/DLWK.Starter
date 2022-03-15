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
    public class ChangePasswordCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    internal class ChangePasswordCommandHandler 
        : IRequestHandler<ChangePasswordCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly UserManager<AppUser> _userManager;

        public ChangePasswordCommandHandler(
            ILogger<ChangePasswordCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<ChangePasswordCommandHandler> localizer,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(
            ChangePasswordCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _userManager.FindByIdAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["User Not Found"]);
                }

                var res = await _userManager.ChangePasswordAsync(entity, command.CurrentPassword, command.NewPassword);

                if (!res.Succeeded)
                {
                    var errors = res.Errors.Select(_ => _.Description).ToArray();
                    return Result<string>.Fail(errors);
                }

                entity.SecurityCode = Helper.CreateRandomPasswordWithRandomLength();
                await _userManager.UpdateAsync(entity);

                return Result<string>.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile user {@0} {UserId}",
                    command, _userSession.UserId);
            }
            return Result<string>.Fail();
        }
    }

    public class ChangePasswordCommandValidator 
        : AbstractValidator<ChangePasswordCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;        

        public ChangePasswordCommandValidator(
            ILogger<ChangePasswordCommandValidator> logger,
            IStringLocalizer<ChangePasswordCommandValidator> localizer)
        {
            _logger = logger;
            _localizer = localizer;

            RuleFor(_ => _.CurrentPassword)
                .NotEmpty().WithMessage(_localizer["You must enter your current password"])
                .MinimumLength(6).WithMessage(_localizer["Current password cannot be less than 6 charaters"]);

            RuleFor(_ => _.NewPassword)
                .NotEmpty().WithMessage(_localizer["You must enter your new password"])
                .MinimumLength(6).WithMessage(_localizer["New password cannot be less than 6 charaters"]);

            RuleFor(_ => _.ConfirmPassword)
                .Equal(_ => _.NewPassword).WithMessage(_localizer["Your confirm password must matched your new password"]);
        }
    }
}
