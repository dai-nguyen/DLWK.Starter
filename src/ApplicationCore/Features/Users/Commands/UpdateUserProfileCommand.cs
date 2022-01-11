using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.Users.Commands
{
    public class UpdateUserProfileCommand : IRequest<Result<string>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
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
                var oldPhone = await _userManager.GetPhoneNumberAsync(entity);

                if (command.Phone != oldPhone)
                {
                    await _userManager.SetPhoneNumberAsync(entity, command.Phone);
                }

                await _userManager.UpdateAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile user {@0} {UserId}",
                    command, _userSession.UserId);
            }
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
        }
    }
}
