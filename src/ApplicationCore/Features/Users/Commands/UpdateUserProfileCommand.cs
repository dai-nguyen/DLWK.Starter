using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Commands
{
    public class UpdateUserProfileCommand : IRequest<Result<string>>
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    internal class UpdateUserProfileCommandHandler 
        : IRequestHandler<UpdateUserProfileCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly UserManager<AppUser> _userManager;

        public UpdateUserProfileCommandHandler(
            ILogger<UpdateUserProfileCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateUserProfileCommandHandler> localizer,
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _userManager = userManager;
        }

        public async Task<Result<string>> Handle(
            UpdateUserProfileCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _userManager.FindByIdAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["User Not Found"]);
                }

                entity.FirstName = command.FirstName;
                entity.LastName = command.LastName;
                //var oldPhone = await _userManager.GetPhoneNumberAsync(entity);
                var oldEmail = await _userManager.GetEmailAsync(entity);

                //if (command.Phone != oldPhone)
                //{
                //    var res = await _userManager.SetPhoneNumberAsync(entity, command.Phone);

                //    if (!res.Succeeded)
                //    {
                //        var errors = res.Errors.Select(_ => _.Description).ToArray();
                //        return Result<string>.Fail(errors);
                //    }
                //}

                if (!string.IsNullOrEmpty(command.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(entity);
                    var res = await _userManager.ResetPasswordAsync(entity, token, command.Password);

                    if (!res.Succeeded)
                    {
                        var errors = res.Errors.Select(_ => _.Description).ToArray();
                        return Result<string>.Fail(errors);
                    }
                }

                if (command.Email != oldEmail)
                {
                    var res = await _userManager.SetEmailAsync(entity, command.Email);

                    if (!res.Succeeded)
                    {
                        var errors = res.Errors.Select(_ => _.Description).ToArray();
                        return Result<string>.Fail(errors);
                    }
                }

                await _userManager.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile user {@0} {UserId}",
                    command, _userSession.UserId);
            }
            return Result<string>.Fail();
        }
    }

    public class UpdateUserProfileCommandValidator 
        : AbstractValidator<UpdateUserProfileCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;        

        public UpdateUserProfileCommandValidator(
            ILogger<UpdateUserProfileCommandValidator> logger,
            IStringLocalizer<UpdateUserProfileCommandValidator> localizer)
        {
            _logger = logger;
            _localizer = localizer;

            RuleSet("Names", () =>
            {
                RuleFor(_ => _.FirstName)
                    .NotEmpty().WithMessage(_localizer["You must enter your first name"])
                    .MaximumLength(50).WithMessage("First name cannot be longer than 50 characters");

                RuleFor(_ => _.LastName)
                    .NotEmpty().WithMessage(_localizer["You must enter your last name"])
                    .MaximumLength(50).WithMessage(_localizer["Last name cannot be longer than 50 characters"]);
            });

            RuleFor(_ => _.Email)
                .NotEmpty().WithMessage(_localizer["You must enter an email address"])
                .EmailAddress().WithMessage(_localizer["You must provide a valid email address"]);

            RuleFor(_ => _.Password)
                .NotEmpty().WithMessage(_localizer["You must enter your password"])
                .MinimumLength(6).WithMessage(_localizer["Password cannot be less than 6 charaters"]);

            RuleFor(_ => _.ConfirmPassword)
                .Equal(_ => _.Password).WithMessage(_localizer["Your confirm password must matched your password"]);
        }
    }
}
