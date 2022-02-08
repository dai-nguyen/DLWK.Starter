using ApplicationCore.Data;
using ApplicationCore.Helpers;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Roles.Commands
{
    public partial class CreateRoleCommand : IRequest<Result<string>>
    {        
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public virtual string ExternalId { get; set; } = "";

        public IEnumerable<RolePermission> Permissions { get; set; } 
            = Enumerable.Empty<RolePermission>();
    }

    internal class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly RoleManager<AppRole> _roleManager;        

        public CreateRoleCommandHandler(
            ILogger<CreateRoleCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateRoleCommandHandler> localizer,
            RoleManager<AppRole> roleManager)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _roleManager = roleManager;            
        }

        public async Task<Result<string>> Handle(
            CreateRoleCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var found = await _roleManager.FindByNameAsync(request.Name);

                if (found != null)
                {
                    return Result<string>.Fail(string.Format(_localizer["Role name {0} is already used."], request.Name));
                }

                var entity = new AppRole()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Description = request.Description,
                };
                
                var created = await _roleManager.CreateAsync(entity);

                if (!created.Succeeded)
                {
                    var errors = created.Errors.Select(_ => _.Description).ToArray();
                    return Result<string>.Fail(errors);
                }

                if (request.Permissions != null && request.Permissions.Any())
                {                 
                    var claims = request.Permissions.ToClaims();

                    foreach (var c in claims)
                    {
                        var rResult = await _roleManager.AddClaimAsync(
                            entity,
                            c);

                        if (!rResult.Succeeded)
                        {
                            _logger.LogError("Error adding claim {0} {UserId}",
                                rResult, _userSession.UserId);
                        }
                    }
                }

                return Result<string>.Success(entity.Id, _localizer["Role Saved"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding role {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer["Internal Error"]);
        }
    }

    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _appDbContext;
        readonly IMemoryCache _cache;

        public CreateRoleCommandValidator(
            ILogger<CreateRoleCommandValidator> logger,
            IStringLocalizer<CreateRoleCommandValidator> localizer,
            AppDbContext appDbContext,
            IMemoryCache cache)
        {
            _logger = logger;
            _localizer = localizer;
            _appDbContext = appDbContext;
            _cache = cache;

            RuleFor(_ => _.Name)
                .NotEmpty().WithMessage(_localizer["You must enter a role name"])
                .Must((name) => IsUniqueRoleName(name))
                .WithMessage(_localizer["Role name must be unique"])
                .When(_ => !string.IsNullOrEmpty(_.Name));

            RuleFor(_ => _.Description)
                .NotEmpty().WithMessage(_localizer["You must enter a description"])
                .MaximumLength(50).WithMessage(_localizer["Description cannot be longer than 50 characters"]);
        }

        private bool IsUniqueRoleName(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(roleName))
                    return false;

                roleName = roleName.Trim();

                return _cache.GetOrCreate(
                    $"IsUniqueRoleName:{roleName.ToLower()}",
                    entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                        return _appDbContext.Roles.Any(_ => _.Name == roleName) == false;
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
