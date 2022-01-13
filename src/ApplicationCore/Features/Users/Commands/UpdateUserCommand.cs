using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using FluentValidation;
using LazyCache;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Commands
{
    public partial class UpdateUserCommand : IRequest<Result<string>>
    {
        public string Id { get; set; }                
        public virtual string ExternalId { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<CustomAttribute> CustomAttributes { get; set; } = Enumerable.Empty<CustomAttribute>();
    }

    internal class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly UserManager<AppUser> _userManager;
        readonly IFileService _fileService;
        readonly IMapper _mapper;

        public UpdateUserCommandHandler(
            ILogger<UpdateUserCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateUserCommandHandler> localizer,
            AppDbContext dbContext,
            UserManager<AppUser> userManager,
            IFileService fileService,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _userManager = userManager;
            _fileService = fileService;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(
            UpdateUserCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _userManager.FindByIdAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["User Not Found"]);
                }

                entity = _mapper.Map(command, entity);
                
                var currentEmail = await _userManager.GetEmailAsync(entity);
                bool newEmail = command.Email != currentEmail;                

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["Unable to map to AppUser"]);
                }

                var updated = await _userManager.UpdateAsync(entity);

                if (!updated.Succeeded)
                {
                    var errors = updated.Errors.Select(_ => _.Description).ToArray();
                    return Result<string>.Fail(errors);
                }

                await UpsertRolesAsync(entity, command);

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

                if (newEmail)
                {
                    //var token = await _userManager.GenerateChangeEmailTokenAsync(entity, command.Email);
                    //var res = await _userManager.ChangeEmailAsync(entity, command.Email, token);

                    //if (!res.Succeeded)
                    //{
                    //    var errors = res.Errors.Select(_ => _.Description).ToArray();
                    //    return Result<string>.Fail(errors);
                    //}

                    var res = await _userManager.SetEmailAsync(entity, command.Email);

                    if (!res.Succeeded)
                    {
                        var errors = res.Errors.Select(_ => _.Description).ToArray();
                        return Result<string>.Fail(errors);
                    }
                }

                return Result<string>.Success(entity.Id, _localizer["User Updated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating User {@0) {UserId}",
                    command, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer["Internal Error"]);
        }

        private async Task UpsertRolesAsync(
            AppUser entity, 
            UpdateUserCommand command)
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

    public class UpdateUserCommandValidator 
        : AbstractValidator<UpdateUserCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _appDbContext;
        readonly IAppCache _cache;

        public UpdateUserCommandValidator(
            ILogger<UpdateUserCommandValidator> logger,
            IStringLocalizer<UpdateUserCommandValidator> localizer,
            AppDbContext appDbContext,
            IAppCache cache)
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
                .EmailAddress().WithMessage(_localizer["You must provide a valid email address"]);

            RuleFor(_ => _.Password)
                .NotEmpty().WithMessage(_localizer["You must enter your password"])
                .MinimumLength(6).WithMessage(_localizer["Password cannot be less than 6 charaters"]);

            RuleFor(_ => _.ConfirmPassword)
                .Equal(_ => _.Password).WithMessage(_localizer["Your confirm password must matched your password"]);
        }
    }
}
