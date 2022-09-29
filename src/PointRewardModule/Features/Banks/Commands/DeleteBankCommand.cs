using ApplicationCore;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PointRewardModule.Data;

namespace PointRewardModule.Features.Banks.Commands
{
    public class DeleteBankCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = "";
    }

    internal class DeleteBankCommandHandler : IRequestHandler<DeleteBankCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly PointRewardModuleDbContext _dbContext;

        public DeleteBankCommandHandler(
            ILogger<DeleteBankCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<DeleteBankCommandHandler> localizer,
            PointRewardModuleDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            DeleteBankCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Banks.FindAsync(request.Id, cancellationToken);

                if (entity == null)
                    return Result<string>.Fail(_localizer[Constants.Messages.NotFound]);

                _dbContext.Banks.Remove(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(_localizer[Constants.Messages.Deleted]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Bank {0} {UserId}",
                    request.Id, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer[Constants.Messages.InternalError]);
        }
    }
}
