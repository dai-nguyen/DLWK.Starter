using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.BulkJobs.Commands
{
    public class UpdateBulkJobCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public IEnumerable<BulkMessageResponse> Messages { get; set; }
        public int Processed { get; set; }
        public int Failed { get; set; }
    }

    internal class UpdateBulkJobCommandHandler : IRequestHandler<UpdateBulkJobCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;

        public UpdateBulkJobCommandHandler(
            ILogger<UpdateBulkJobCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateBulkJobCommandHandler> localizer,
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            UpdateBulkJobCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.BulkJobs.FindAsync(request.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["BulkJob Not Found"]);
                }

                entity.Status = request.Status;
                entity.Error = System.Text.Json.JsonSerializer.Serialize(request.Messages);
                entity.Processed = request.Processed;
                entity.Failed = request.Failed;

                await _dbContext.SaveChangesAsync();

                return Result<string>.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating BulkJob {@0) {UserId}",
                    request, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer[Constants.Messages.InternalError]);
        }
    }
}
