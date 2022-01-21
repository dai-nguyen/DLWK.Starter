using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Roles.Commands
{
    public class UpdateRoleCommand : IRequest<Result<string>>
    {
        public virtual string ExternalId { get; set; } = "";

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    internal class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly RoleManager<AppRole> _roleManager;
        readonly IMapper _mapper;

        public UpdateRoleCommandHandler(
            ILogger<UpdateRoleCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateRoleCommandHandler> localizer,
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

                entity.Name = request.Name;
                entity.Description = request.Description;
                
                var updated = await _roleManager.UpdateAsync(entity);

                if (!updated.Succeeded)
                {
                    var errors = updated.Errors.Select(_ => _.Description).ToArray();
                    return Result<string>.Fail(errors);
                }

                return Result<string>.Success(entity.Id, _localizer["Role Updated"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer["Internal Error"]);
        }
    }
}
