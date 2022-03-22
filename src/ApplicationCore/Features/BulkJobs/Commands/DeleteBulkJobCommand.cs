using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.BulkJobs.Commands
{
    public class DeleteBulkJobCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = string.Empty;
    }

    internal class DeleteBulkJobCommandHandler : IRequestHandler<DeleteBulkJobCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;

        public DeleteBulkJobCommandHandler(
            ILogger<DeleteBulkJobCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<DeleteBulkJobCommandHandler> localizer,
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            DeleteBulkJobCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.BulkJobs.FindAsync(request.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["BulkJob Not Found"]);
                }

                _dbContext.BulkJobs.Remove(entity);
                await _dbContext.SaveChangesAsync();
                return Result<string>.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting BulkJob {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer[Constants.Messages.InternalError]);
        }
    }
}
