﻿using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Contacts.Commands
{
    public class DeleteContactCommand : IRequest<Result<string>>
    {
        public string Id { get; set; }
    }

    internal class DeleteContactCommandHandler :
        IRequestHandler<DeleteContactCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;

        public DeleteContactCommandHandler(
            ILogger<DeleteContactCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<DeleteContactCommandHandler> localizer,
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            DeleteContactCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Contacts.FindAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer[Const.Messages.NotFound]);
                }

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    // unlink projects
                    await _dbContext.Projects
                        .Where(_ => _.ContactId == command.Id)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(_ => _.ContactId, (string?)null));

                    _dbContext.Contacts.Remove(entity);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    transaction.Commit();
                }

                return Result<string>.Success(entity.Id,
                    _localizer[Const.Messages.Deleted]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {@0} {UserId}",
                    command, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer[Const.Messages.InternalError]);
        }
    }
}
