using ApplicationCore.Data;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Commands
{
    public class RegisterUserCommand : IRequest<Result<string>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        //public string PhoneNumber { get; set; }
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
                    FirstName = command.FirstName.Trim(),
                    LastName = command.LastName.Trim(),
                    UserName = command.UserName.Trim(),
                    Email = command.Email.Trim(),
                };

                var result = await _userManager.CreateAsync(entity, command.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(_ => _.Description).ToArray();
                    return Result<string>.Fail(errors);
                }

                await _userManager.AddToRoleAsync(entity, "User");
                return Result<string>.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {@0}",
                    command);
            }
            return Result<string>.Fail(_localizer["Internal Error"]);
        }
    }

    public class RegisterUserCommandValidator 
        : AbstractValidator<RegisterUserCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;        
        readonly AppDbContext _appDbContext;
        readonly IMemoryCache _cache;

        public RegisterUserCommandValidator(
            ILogger<RegisterUserCommandValidator> logger,
            IStringLocalizer<RegisterUserCommandValidator> localizer,        
            AppDbContext appDbContext,
            IMemoryCache cache)            
        {
            _logger = logger;
            _localizer = localizer;            
            _appDbContext = appDbContext;
            _cache = cache;

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
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(email))
                    return false;

                email = email.Trim();             

                return _cache.GetOrCreate(
                    $"IsUniqueUserEmail:{email.Trim().ToLower()}",
                    entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                        return _appDbContext.Users.Any(_ => _.Email == email) == false;
                    });
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
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(username))
                    return false;

                username = username.Trim();

                return _cache.GetOrCreate(
                    $"IsUniqueUsername:{username.Trim().ToLower()}",
                    entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                        return _appDbContext.Users.Any(_ => _.UserName == username) == false;
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking unique username");
            }
            return false;
        }
    }
}
