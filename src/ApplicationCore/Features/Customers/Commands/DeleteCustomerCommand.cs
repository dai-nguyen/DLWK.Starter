using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Customers.Commands
{
    public class DeleteCustomerCommand : IRequest<Result<string>>
    {
        public string Id { get; set; }
    }

    internal class DeleteCustomerDemandHandler :
        IRequestHandler<DeleteCustomerCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;

        public DeleteCustomerDemandHandler(
            ILogger<DeleteCustomerDemandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<DeleteCustomerDemandHandler> localizer,
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            DeleteCustomerCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Customers.FindAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer[Const.Messages.NotFound]);
                }                

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    // remove contacts
                    await _dbContext.Contacts.Where(_ => _.CustomerId == command.Id)
                        .ExecuteDeleteAsync(cancellationToken);

                    // remove projects
                    await _dbContext.Projects.Where(_ => _.CustomerId == command.Id)
                        .ExecuteDeleteAsync(cancellationToken);

                    // remove customer
                    _dbContext.Customers.Remove(entity);
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
