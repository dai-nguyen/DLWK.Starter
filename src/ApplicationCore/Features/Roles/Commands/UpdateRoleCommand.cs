using ApplicationCore.Data;
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
        public string Description { get; set; } = "";

        public virtual string ExternalId { get; set; } = "";

        public IEnumerable<AppClaim> Claims { get; set; } 
            = Enumerable.Empty<AppClaim>();
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

                await UpsertClaimsAsync(entity, request);

                return Result<string>.Success(entity.Id, _localizer["Role Updated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer["Internal Error"]);
        }

        private async Task UpsertClaimsAsync(
            AppRole entity,
            UpdateRoleCommand command)
        {
            try
            {
                if (command.Claims == null)
                    command.Claims = Enumerable.Empty<AppClaim>();

                command.Claims = command.Claims
                    .Where(_ => !string.IsNullOrWhiteSpace(_.Type) && !string.IsNullOrWhiteSpace(_.Value))
                    .ToArray();

                var claims = await _roleManager.GetClaimsAsync(entity);

                // add or update
                foreach (var claim in command.Claims)
                {
                    var found = claims.FirstOrDefault(_ => _.Type == claim.Type);

                    // update
                    if ((found != null && found.Value != claim.Value) || found == null)
                    {
                        if (found != null && found.Value != claim.Value)
                            await _roleManager.RemoveClaimAsync(entity, found);

                        await _roleManager.AddClaimAsync(entity, new System.Security.Claims.Claim(claim.Type, claim.Value));
                    }
                }

                // remove
                foreach (var claim in claims)
                {
                    var found = command.Claims.Any(_ => _.Type == claim.Type);

                    if (found) continue;

                    await _roleManager.RemoveClaimAsync(entity, claim);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upsert role claims {@0} {UserId}",
                    command, _userSession.UserId);
            }
        }
    }

    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;        
        
        public UpdateRoleCommandValidator(
            ILogger<UpdateRoleCommandValidator> logger,
            IStringLocalizer<UpdateRoleCommandValidator> localizer)
        {
            _logger = logger;
            _localizer = localizer;            
            
            RuleFor(_ => _.Description)
                .NotEmpty().WithMessage(_localizer["You must enter a description"])
                .MaximumLength(50).WithMessage(_localizer["Description cannot be longer than 50 characters"]);
        }
    }
}
