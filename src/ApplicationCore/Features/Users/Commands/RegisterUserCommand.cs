using ApplicationCore.Data;
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
    public class RegisterUserCommand : IRequest<Result<IEnumerable<string>>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
        public bool ActivateUser { get; set; } = false;
        public bool AutoConfirmEmail { get; set; } = false;
        public bool AgreeToTerms { get; set; } = false;
    }

    internal class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<IEnumerable<string>>>
    {
        readonly ILogger _logger;        
        readonly IStringLocalizer _localizer;        
        readonly UserManager<AppUser> _userManager;        

        public RegisterUserCommandHandler(
            ILogger<RegisterUserCommandHandler> logger,            
            IStringLocalizer<RegisterUserCommandHandler> localizer,            
            UserManager<AppUser> userManager)
        {
            _logger = logger;            
            _localizer = localizer;            
            _userManager = userManager;            
        }

        public async Task<Result<IEnumerable<string>>> Handle(
            RegisterUserCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = new AppUser()
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    UserName = command.UserName,
                    Email = command.Email,
                };

                var result = await _userManager.CreateAsync(entity, command.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(_ => _.Description).ToArray();

                    return Result<IEnumerable<string>>.Fail(errors);
                }
                else
                {
                    await _userManager.AddToRoleAsync(entity, "User");

                    return Result<IEnumerable<string>>.Success();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {0}",
                    command);
            }

            return Result<IEnumerable<string>>.Fail(_localizer["Internal Error"]);
        }
    }

    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        readonly IStringLocalizer _localizer;
        readonly UserManager<AppUser> _userManager;

        public RegisterUserCommandValidator(
            IStringLocalizer<RegisterUserCommandValidator> localizer,
            UserManager<AppUser> userManager)
        {
            _localizer = localizer;
            _userManager = userManager;

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
                .EmailAddress().WithMessage(_localizer["You must provide a valid email address"])
                .MustAsync(async (email, cancellationToken) => await IsUniqueEmailAsync(email))
                .WithMessage(_localizer["Email address must be unique"])
                .When(_ => !string.IsNullOrEmpty(_.Email));

            RuleFor(_ => _.UserName)
                .NotEmpty().WithMessage(_localizer[""])
                .MinimumLength(6).WithMessage(_localizer[""])
                .MustAsync(async (username, cancellationToken) => await IsUniqueUsernameAsync(username))
                .WithMessage(_localizer["Username is already used."])
                .When(_ => !string.IsNullOrEmpty(_.UserName)); ;

            RuleFor(_ => _.Password)
                .NotEmpty().WithMessage(_localizer[""])
                .MinimumLength(6).WithMessage(_localizer[""])
                .Equal(_ => _.ConfirmPassword).WithMessage(_localizer[""]);

            RuleFor(_ => _.ConfirmPassword)
                .Equal(_ => _.Password).WithMessage(_localizer[""]);

            RuleFor(_ => _.AgreeToTerms)
                .Equal(true);


        }

        private async Task<bool> IsUniqueEmailAsync(string email)
        {
            var found = await _userManager.FindByEmailAsync(email);
            return found == null;
        }

        private async Task<bool> IsUniqueUsernameAsync(string username)
        {
            var found = await _userManager.FindByNameAsync(username);
            return found == null;
        }
    }
}
