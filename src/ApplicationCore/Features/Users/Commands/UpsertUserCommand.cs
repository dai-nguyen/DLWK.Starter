using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Commands
{
    public partial class UpsertUserCommand : IRequest<Result<string>>
    {
        public string Id { get; set; }                
        public virtual string ExternalId { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }

        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<CustomAttribute> CustomAttributes { get; set; } = Enumerable.Empty<CustomAttribute>();
    }

    internal class UpsertUserCommandHandler : IRequestHandler<UpsertUserCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSession _userSession;
        readonly IStringLocalizer<UpsertUserCommandHandler> _localizer;
        readonly AppDbContext _dbContext;
        readonly UserManager<AppUser> _userManager;
        readonly IFileService _fileService;
        readonly IMapper _mapper;

        public UpsertUserCommandHandler(
            ILogger<UpsertUserCommandHandler> logger,
            IUserSession userSession,
            IStringLocalizer<UpsertUserCommandHandler> localizer,
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
            UpsertUserCommand command, 
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Id))
            {
                return await AddAsync(command);
            }
            else
            {
                return await UpdateAsync(command);
            }
        }

        private async Task<Result<string>> AddAsync(UpsertUserCommand command)
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

        private async Task<Result<string>> UpdateAsync(UpsertUserCommand command)
        {
            try
            {
                var entity = await _userManager.FindByIdAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["User Not Found"]);
                }

                entity = _mapper.Map(command, entity);

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

                if (!string.IsNullOrEmpty(command.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(entity);
                    var passReset = await _userManager.ResetPasswordAsync(entity, token, command.Password);
                }

                await UpsertRolesAsync(entity, command);

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
            UpsertUserCommand command)
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
}
