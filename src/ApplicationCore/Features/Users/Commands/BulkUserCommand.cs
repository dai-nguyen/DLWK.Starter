using ApplicationCore.Data;
using ApplicationCore.Helpers;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ApplicationCore.Features.Users.Commands
{
    public class BulkUser : BulkBaseModel
    {
        public string UserName { get; set; } = string.Empty;        
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }

    public class BulkUserCommand : IRequest<Result<IEnumerable<string>>>
    {
        public IEnumerable<BulkUser> Users { get; set; } = Enumerable.Empty<BulkUser>();
    }

    internal class BulkUserCommandHandler : IRequestHandler<BulkUserCommand, Result<IEnumerable<string>>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly UserManager<AppUser> _userManager;
        readonly IMediator _mediator;

        public BulkUserCommandHandler(
            ILogger<BulkUserCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<BulkUserCommandHandler> localizer,
            AppDbContext dbContext,
            UserManager<AppUser> userManager,
            IMediator mediator)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _userManager = userManager;            
            _mediator = mediator;
        }

        public async Task<Result<IEnumerable<string>>> Handle(
            BulkUserCommand request, 
            CancellationToken cancellationToken)
        {
            var list = new List<string>();
            int processed = 0;
            int failed = 0;


            try
            {
                var permission = _userSession.Claims.GetPermission(Constants.ClaimNames.users);

                if (!permission.can_bulk)
                    return Result<IEnumerable<string>>.Fail(_localizer[Constants.Messages.PermissionDenied]);

                foreach (var r in request.Users)
                {
                    string operation = string.Empty;

                    if (r.Operation == BulkOperation.Upsert)
                    {
                        operation = await UpsertAsync(r);
                        
                    }
                    else if (r.Operation == BulkOperation.Delete)
                    {
                        operation = await DeleteAsync(r);
                    }            
                    
                    list.Add(operation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk processing users {@0) {UserId}",
                    request, _userSession.UserId);
            }

            return Result<IEnumerable<string>>.Success(list);
        }

        async Task<string> UpsertAsync(BulkUser request)
        {
            try
            {
                AppUser entity = null;

                if (!string.IsNullOrEmpty(request.UserName))
                {
                    entity = await _userManager.FindByNameAsync(request.UserName);
                }

                if (entity == null && !string.IsNullOrEmpty(request.ExternalId))
                {
                    entity = await _dbContext.Users
                        .Where(_ => !string.IsNullOrEmpty(_.ExternalId))
                        .FirstOrDefaultAsync(_ => _.ExternalId == request.ExternalId);
                }

                if (entity == null)
                {
                    // add
                    var command = new CreateUserCommand()
                    {
                        UserName = request.UserName,
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Title = request.Title,
                        Password = request.Password,
                        ConfirmPassword = request.Password,
                        ExternalId = request.ExternalId,
                        Roles = request.Roles,
                    };

                    var added = await _mediator.Send(command);

                    return $"{JsonSerializer.Serialize(request)}|add|{JsonSerializer.Serialize(added)}";
                }
                else if (entity != null)
                {
                    // update
                    var command = new UpdateUserCommand()
                    {
                        Id = entity.Id,
                        UserName = request.UserName,
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName= request.LastName,
                        Title = request.Title,
                        ExternalId= request.ExternalId,
                        Roles= request.Roles,
                    };

                    if (!string.IsNullOrWhiteSpace(request.Password))
                    {
                        command.Password = request.Password;
                        command.ConfirmPassword = request.Password;
                    }

                    var updated = await _mediator.Send(command);

                    return $"{JsonSerializer.Serialize(request)}|update|{JsonSerializer.Serialize(updated)}";
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk processing user {@0) {UserId}",
                    request, _userSession.UserId);
            }

            return $"{JsonSerializer.Serialize(request)}|error|";
        }

        async Task<string> DeleteAsync(BulkUser request)
        {
            try
            {
                AppUser entity = null;

                if (!string.IsNullOrEmpty(request.UserName))
                {
                    entity = await _userManager.FindByNameAsync(request.UserName);
                }

                if (entity == null && !string.IsNullOrEmpty(request.ExternalId))
                {
                    entity = await _dbContext.Users
                        .Where(_ => !string.IsNullOrEmpty(_.ExternalId))
                        .FirstOrDefaultAsync(_ => _.ExternalId == request.ExternalId);
                }

                if (entity == null)
                {
                    return $"{JsonSerializer.Serialize(request)}|delete|not found";
                }

                var command = new DeleteUserCommand()
                {
                    Id = entity.Id
                };

                var deleted = await _mediator.Send(command);

                return $"{JsonSerializer.Serialize(request)}|delete|{JsonSerializer.Serialize(deleted)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk processing user {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return $"{JsonSerializer.Serialize(request)}|error|";
        }
    }
}
