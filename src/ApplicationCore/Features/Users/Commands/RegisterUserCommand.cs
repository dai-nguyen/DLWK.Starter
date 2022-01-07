using ApplicationCore.Data;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ApplicationCore.Features.Users.Commands
{
    public class RegisterUserCommand : IRequest<Result<string>>
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

    internal class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<string>>
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

        public async Task<Result<string>> Handle(
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

                    return Result<string>.Fail(errors);
                }
                else
                {
                    await _userManager.AddToRoleAsync(entity, "User");

                    return Result<string>.Success();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {0}",
                    command);
            }

            return Result<string>.Fail(_localizer["Internal Error"]);
        }
    }

    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;        
        readonly AppDbContext _appDbContext;

        public RegisterUserCommandValidator(
            ILogger<RegisterUserCommandValidator> logger,
            IStringLocalizer<RegisterUserCommandValidator> localizer,        
            AppDbContext appDbContext)
        {
            _localizer = localizer;            
            _appDbContext = appDbContext;

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
                .Must((email) => IsUniqueEmail(email))
                .WithMessage(_localizer["Email address must be unique"])
                .When(_ => !string.IsNullOrEmpty(_.Email));

            RuleFor(_ => _.UserName)
                .NotEmpty().WithMessage(_localizer["You must enter a username"])
                .MinimumLength(6).WithMessage(_localizer["Username cannot be less than 6 characters"])
                .Must((username) => IsUniqueUsername(username))                
                .WithMessage(_localizer["Username must be unique"])
                .When(_ => !string.IsNullOrEmpty(_.UserName));

            RuleFor(_ => _.Password)
                .NotEmpty().WithMessage(_localizer["You must enter your password"])
                .MinimumLength(6).WithMessage(_localizer["Password cannot be less than 6 charaters"]);

            RuleFor(_ => _.ConfirmPassword)
                .Equal(_ => _.Password).WithMessage(_localizer["Your confirm password must matched your password"]);

            RuleFor(_ => _.AgreeToTerms)
                .Equal(true).WithMessage("You must agree to terms and conditions");
        }

        private bool IsUniqueEmail(string email)
        {
            try
            {
                var found = _appDbContext.Users.Any(_ => _.Email == email);
                return found == false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for unique email");
            }
            return false;
        }

        private bool IsUniqueUsername(string username)
        {
            try
            {
                var found = _appDbContext.Users.Any(_ => _.UserName == username);
                return found == false;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error checking unique username");
            }
            return false;
        }
    }
}
