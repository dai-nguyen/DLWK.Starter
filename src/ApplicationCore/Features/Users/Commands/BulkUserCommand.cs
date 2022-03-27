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
    public class BulkUser : BulkModelBase
    {
        public string UserName { get; set; } = string.Empty;        
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }

    public class BulkUserCommand : IRequest<Result<BulkResponseBase>>
    {
        public IEnumerable<BulkUser> Users { get; set; } = Enumerable.Empty<BulkUser>();
    }

    internal class BulkUserCommandHandler : IRequestHandler<BulkUserCommand, Result<BulkResponseBase>>
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

        public async Task<Result<BulkResponseBase>> Handle(
            BulkUserCommand request, 
            CancellationToken cancellationToken)
        {
            var messages = new List<BulkMessageResponse>();
            int processed = 0;
            int failed = 0;

            try
            {
                var permission = _userSession.Claims.GetPermission(Constants.ClaimNames.users);

                if (!permission.can_bulk)
                    return Result<BulkResponseBase>.Fail(_localizer[Constants.Messages.PermissionDenied]);

                foreach (var r in request.Users)
                {
                    Result<string> operation = null;

                    if (r.Operation == BulkOperation.Upsert)
                    {
                        operation = await UpsertAsync(r);
                        
                    }
                    else if (r.Operation == BulkOperation.Delete)
                    {
                        operation = await DeleteAsync(r);
                    }            
                    
                    list.Add(operation.Data);

                    if (operation.Succeeded)
                        processed += 1;
                    else
                        failed += 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk processing users {@0) {UserId}",
                    request, _userSession.UserId);
            }

            var data = new BulkResponseBase()
            {
                Messages = list,
                Processed = processed,
                Failed = failed
            };

            return Result<BulkResponseBase>.Success(data);
        }

        async Task<Result<BulkMessageResponse>> UpsertAsync(BulkUser request)
        {
            var message = new BulkMessageResponse();
            message.Request = JsonSerializer.Serialize(request);

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
                    message.Operation = "add";

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

                    var addedRes = await _mediator.Send(command);

                    message.Response = JsonSerializer.Serialize(addedRes);

                    if (addedRes.Succeeded)
                        return Result<BulkMessageResponse>.Success(message);
                    else
                        return Result<BulkMessageResponse>.Fail(message);
                }
                else if (entity != null)
                {
                    message.Operation = "update";

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

                    var updatedRes = await _mediator.Send(command);
                    message.Response = JsonSerializer.Serialize(updatedRes);

                    if (updatedRes.Succeeded)
                        return Result<BulkMessageResponse>.Success(message);
                    else
                        return Result<BulkMessageResponse>.Fail(message);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk processing user {@0) {UserId}",
                    request, _userSession.UserId);
            }

            return Result<BulkMessageResponse>.Fail(message);
        }

        async Task<Result<BulkMessageResponse>> DeleteAsync(BulkUser request)
        {
            var message = new BulkMessageResponse();
            message.Request = JsonSerializer.Serialize(request);
            message.Operation = "delete";

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
                    message.Response = "not found";
                    return Result<BulkMessageResponse>.Fail(message);
                }

                var command = new DeleteUserCommand()
                {
                    Id = entity.Id
                };

                var deletedRes = await _mediator.Send(command);

                message.Response = JsonSerializer.Serialize(deletedRes);

                if (deletedRes.Succeeded)
                    return Result<BulkMessageResponse>.Success(message);
                else
                    return Result<BulkMessageResponse>.Fail(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk processing user {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return Result<BulkMessageResponse>.Fail(message);
        }
    }
}
