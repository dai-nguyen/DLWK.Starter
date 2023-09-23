using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Helpers;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.BulkJobs.Commands
{
    public partial class CreateBulkJobCommand : IRequest<Result<string>>
    {
        public string EntityName { get; set; } = String.Empty;
    }

    internal class CreateBulkJobCommandHandler : IRequestHandler<CreateBulkJobCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;

        public CreateBulkJobCommandHandler(
            ILogger<CreateBulkJobCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateBulkJobCommandHandler> localizer,
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            CreateBulkJobCommand request, 
            CancellationToken cancellationToken)
        {
            var permission = _userSession.Claims.GetPermission(request.EntityName);

            if (permission == null || (permission != null && !permission.can_bulk))
                return Result<string>.Fail(_localizer[Const.Messages.PermissionDenied]);

            try
            {
                var entity = new BulkJob();
                entity.Id = Guid.NewGuid().ToString();
                entity.Status = Const.BulkJobStatus.Pending;
                entity.Processed = 0;
                entity.Failed = 0;

                _dbContext.BulkJobs.Add(entity);
                int count = await _dbContext.SaveChangesAsync(cancellationToken);

                if (count > 0)
                    return Result<string>.Success(entity.Id, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding BulkJob {@0) {UserId}",
                    request, _userSession.UserId);
            }
            
            return Result<string>.Fail(_localizer[Const.Messages.InternalError]);
        }
    }


}
