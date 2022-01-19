using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Commands
{
    public partial class CreateUserCommand : IRequest<Result<string>>
    {        
        public virtual string ExternalId { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<CustomAttribute> CustomAttributes { get; set; } = Enumerable.Empty<CustomAttribute>();
    }

    internal class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;        
        readonly UserManager<AppUser> _userManager;        
        readonly IMapper _mapper;

        public CreateUserCommandHandler(
            ILogger<CreateUserCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateUserCommandHandler> localizer,            
            UserManager<AppUser> userManager,            
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;            
            _userManager = userManager;            
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(
            CreateUserCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var found = await _userManager.FindByNameAsync(command.UserName);

                if (found != null)
                {
                    return Result<string>.Fail(string.Format(_localizer["UserName {0} is already used."], command.UserName));
                }

                found = await _userManager.FindByEmailAsync(command.Email);

                if (found != null)
                {
                    return Result<string>.Fail(string.Format(_localizer["Email {0} is already used."], command.Email));
                }

                var entity = _mapper.Map<AppUser>(command);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["Unable to map to AppUser"]);
                }

                entity.Id = Guid.NewGuid().ToString();

                var created = await _userManager.CreateAsync(entity, command.Password);

                if (!created.Succeeded)
                {
                    var errors = created.Errors.Select(_ => _.Description).ToArray();

                    return Result<string>.Fail(errors);
                }

                await UpsertRolesAsync(entity, command);

                return Result<string>.Success(entity.Id, _localizer["User Saved"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding User {@0) {UserId}",
                    command, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer["Internal Error"]);
        }

        private async Task UpsertRolesAsync(
            AppUser entity, 
            CreateUserCommand command)
        {
            if (command.Roles == null)
                command.Roles = new List<string>();

            var roles = await _userManager.GetRolesAsync(entity);

            // add
            foreach (var role in command.Roles)
            {
                var found = roles.FirstOrDefault(_ => _ == role);

                if (found != null) continue;

                await _userManager.AddToRoleAsync(entity, role);
            }

            // remove
            foreach (var role in roles)
            {
                var found = command.Roles.Any(_ => _ == role);

                if (found) continue;

                await _userManager.RemoveFromRoleAsync(entity, role);
            }
        }
    }

    public class CreateUserCommandValidator 
        : AbstractValidator<CreateUserCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _appDbContext;        
        readonly IMemoryCache _cache;

        public CreateUserCommandValidator(
            ILogger<CreateUserCommandValidator> logger,
            IStringLocalizer<CreateUserCommandValidator> localizer,
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
                    .NotEmpty().WithMessage(_localizer["You must enter first name"])
                    .MaximumLength(50).WithMessage("First name cannot be longer than 50 characters");

                RuleFor(_ => _.LastName)
                    .NotEmpty().WithMessage(_localizer["You must enter last name"])
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
