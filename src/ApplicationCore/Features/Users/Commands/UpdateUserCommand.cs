using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Helpers;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Users.Commands
{
    public class UpdateUserCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        public virtual string ExternalId { get; set; } = string.Empty;

        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();        
    }

    internal class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;        
        readonly UserManager<AppUser> _userManager;        

        public UpdateUserCommandHandler(
            ILogger<UpdateUserCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateUserCommandHandler> localizer,            
            UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;            
            _userManager = userManager;            
        }

        public async Task<Result<string>> Handle(
            UpdateUserCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var permission = _userSession.Claims.GetPermission(Const.ClaimNames.users);

                if (!permission.can_edit)
                    return Result<string>.Fail(_localizer[Const.Messages.PermissionDenied]);

                var entity = await _userManager.FindByIdAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["User Not Found"]);
                }
                
                var currentEmail = await _userManager.GetEmailAsync(entity);
                bool newEmail = command.Email != currentEmail;

                entity.FirstName = command.FirstName;
                entity.LastName = command.LastName;
                entity.Title = command.Title;
                entity.ExternalId = command.ExternalId;

                if (!string.IsNullOrWhiteSpace(command.Password))
                    entity.SecurityCode = Helper.CreateRandomPasswordWithRandomLength();

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
            try
            {
                if (command.Roles == null)
                    command.Roles = Enumerable.Empty<string>();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upsert user role {@0} {UserId}",
                    command, _userSession.UserId);
            }
        }

        //private async Task UpsertClaimsAsync(
        //    AppUser entity,
        //    UpdateUserCommand command)
        //{
        //    try
        //    {
        //        if (command.Claims == null)
        //            command.Claims = Enumerable.Empty<AppClaim>();

        //        command.Claims = command.Claims
        //            .Where(_ => !string.IsNullOrWhiteSpace(_.Type) && !string.IsNullOrWhiteSpace(_.Value))
        //            .ToArray();

        //        var claims = await _userManager.GetClaimsAsync(entity);

        //        // add or update
        //        foreach (var claim in command.Claims)
        //        {
        //            var found = claims.FirstOrDefault(_ => _.Type == claim.Type);

        //            // update
        //            if ((found != null && found.Value != claim.Value) || found == null)
        //            {
        //                if (found != null && found.Value != claim.Value)
        //                    await _userManager.RemoveClaimAsync(entity, found);

        //                await _userManager.AddClaimAsync(entity, new System.Security.Claims.Claim(claim.Type, claim.Value));
        //            }
        //        }

        //        // remove
        //        foreach (var claim in claims)
        //        {
        //            var found = command.Claims.Any(_ => _.Type == claim.Type);

        //            if (found) continue;

        //            await _userManager.RemoveClaimAsync(entity, claim);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error upsert user claims {@0} {UserId}",
        //            command, _userSession.UserId);
        //    }
        //}
    }

    public class UpdateUserCommandValidator 
        : AbstractValidator<UpdateUserCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _appDbContext;        

        public UpdateUserCommandValidator(
            ILogger<UpdateUserCommandValidator> logger,
            IStringLocalizer<UpdateUserCommandValidator> localizer,
            AppDbContext appDbContext)
        {
            _logger = logger;
            _localizer = localizer;
            _appDbContext = appDbContext;            

            RuleSet("Names", () =>
            {
                RuleFor(_ => _.FirstName)
                    .NotEmpty().WithMessage(_localizer["You must enter first name"])
                    .MaximumLength(50).WithMessage("First name cannot be longer than 50 characters");

                RuleFor(_ => _.LastName)
                    .NotEmpty().WithMessage(_localizer["You must enter last name"])
                    .MaximumLength(50).WithMessage(_localizer["Last name cannot be longer than 50 characters"]);

                RuleFor(_ => _.Title)
                    .NotEmpty().WithMessage(_localizer["You must enter title"])
                    .MaximumLength(50).WithMessage(_localizer["Title cannot not be longer than 50 characters"]);
            });

            RuleFor(_ => _.Email)
                .NotEmpty().WithMessage(_localizer["You must enter an email address"])
                .EmailAddress().WithMessage(_localizer["You must provide a valid email address"]);

            //RuleFor(_ => _.Password)
            //    .NotEmpty().WithMessage(_localizer["You must enter your password"])
            //    .MinimumLength(6).WithMessage(_localizer["Password cannot be less than 6 charaters"]);

            RuleFor(_ => _.ConfirmPassword)
                .Equal(_ => _.Password).WithMessage(_localizer["Your confirm password must matched your password"]);
        }
    }
}
