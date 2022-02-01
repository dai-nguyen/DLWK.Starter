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
        readonly IMapper _mapper;

        public CreateRoleCommandHandler(
            ILogger<CreateRoleCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateRoleCommandHandler> localizer,
            RoleManager<AppRole> roleManager,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _roleManager = roleManager;
            _mapper = mapper;
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

                var entity = _mapper.Map<AppRole>(request);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["Unable to map to AppRole"]);
                }

                entity.Id = Guid.NewGuid().ToString();

                var created = await _roleManager.CreateAsync(entity);

                if (!created.Succeeded)
                {
                    var errors = created.Errors.Select(_ => _.Description).ToArray();
                    return Result<string>.Fail(errors);
                }

                if (request.Permissions != null && request.Permissions.Any())
                {                    
                    foreach (var p in request.Permissions)
                    {
                        if (string.IsNullOrEmpty(p.name))
                        {
                            _logger.LogError("Permission Name is required. {@0} {UserId}",
                                p, _userSession.UserId);
                            continue;
                        }



                        var rResult = await _roleManager.AddClaimAsync(entity, 
                            new System.Security.Claims.Claim(p.name, "true"));

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
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
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
