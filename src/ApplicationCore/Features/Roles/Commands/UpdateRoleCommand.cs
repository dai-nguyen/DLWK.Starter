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

namespace ApplicationCore.Features.Roles.Commands
{
    public class UpdateRoleCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public virtual string ExternalId { get; set; } = "";

        public IEnumerable<RolePermission> Permissions { get; set; }
            = Enumerable.Empty<RolePermission>();
    }

    internal class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly RoleManager<AppRole> _roleManager;        

        public UpdateRoleCommandHandler(
            ILogger<UpdateRoleCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateRoleCommandHandler> localizer,
            RoleManager<AppRole> roleManager)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _roleManager = roleManager;            
        }

        public async Task<Result<string>> Handle(
            UpdateRoleCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var permission = _userSession.Claims.GetPermission(Const.ClaimNames.roles);

                if (!permission.can_edit)
                    return Result<string>.Fail(_localizer[Const.Messages.PermissionDenied]);

                var entity = await _roleManager.FindByIdAsync(request.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["Role Not Found"]);
                }
                
                entity.Description = request.Description;
                entity.ExternalId = request.ExternalId;
                
                var updated = await _roleManager.UpdateAsync(entity);

                if (!updated.Succeeded)
                {
                    var errors = updated.Errors.Select(_ => _.Description).ToArray();
                    return Result<string>.Fail(errors);
                }

                var res = await UpsertClaimsAsync(entity, request);

                if (res.Succeeded)
                    return Result<string>.Success(entity.Id, _localizer["Role Updated"]);

                return Result<string>.Fail(res.Messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer["Internal Error"]);
        }

        private async Task<Result<bool>> UpsertClaimsAsync(
            AppRole entity,
            UpdateRoleCommand command)
        {
            try
            {
                if (command.Permissions == null)
                    command.Permissions = Enumerable.Empty<RolePermission>();

                var oldClaims = await _roleManager.GetClaimsAsync(entity);
                var newClaims = command.Permissions.ToClaims();

                var errors = new List<string>();

                // add or update
                foreach (var nClaim in newClaims)
                {
                    var oFound = oldClaims.FirstOrDefault(_ => _.Type == nClaim.Type);

                    // update
                    if ((oFound != null && oFound.Value != nClaim.Value) || oFound == null)
                    {
                        if (oFound != null && oFound.Value != nClaim.Value)
                            await _roleManager.RemoveClaimAsync(entity, oFound);

                        var res = await _roleManager.AddClaimAsync(entity, nClaim);

                        if (!res.Succeeded)
                        {
                            errors.AddRange(res.Errors.Select(_ => _.Description));
                        }
                    }
                }

                // remove
                foreach (var claim in oldClaims)
                {
                    var found = newClaims.Any(_ => _.Type == claim.Type);

                    if (found) continue;

                    var res = await _roleManager.RemoveClaimAsync(entity, claim);

                    if (!res.Succeeded)
                    {
                        errors.AddRange(res.Errors.Select(_ => _.Description));
                    }
                }

                if (errors.Any())
                    return Result<bool>.Fail(errors);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upsert role claims {@0} {UserId}",
                    command, _userSession.UserId);
            }
            return Result<bool>.Fail(_localizer["Internal Error"]);
        }
    }

    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {        
        readonly IStringLocalizer _localizer;        
        
        public UpdateRoleCommandValidator(            
            IStringLocalizer<UpdateRoleCommandValidator> localizer)
        {            
            _localizer = localizer;            
            
            RuleFor(_ => _.Description)
                .NotEmpty().WithMessage(_localizer["You must enter a description"])
                .MaximumLength(50).WithMessage(_localizer["Description cannot be longer than 50 characters"]);
        }
    }
}
