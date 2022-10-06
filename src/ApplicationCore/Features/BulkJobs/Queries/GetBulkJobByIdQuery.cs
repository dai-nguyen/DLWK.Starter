using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.BulkJobs.Queries
{
    public class GetBulkJobByIdQuery : IRequest<Result<GetBulkJobByIdQueryResponse>>
    {
        public string Id { get; set; } = "";
    }

    internal class GetBulkJobByIdQueryHandler : IRequestHandler<GetBulkJobByIdQuery, Result<GetBulkJobByIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly IMemoryCache _cache;
        readonly AppDbContext _dbContext;

        public GetBulkJobByIdQueryHandler(
            ILogger<GetBulkJobByIdQueryHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<GetBulkJobByIdQueryHandler> localizer,            
            IMemoryCache cache,
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;            
            _cache = cache;
            _dbContext = dbContext;
        }

        public async Task<Result<GetBulkJobByIdQueryResponse>> Handle(
            GetBulkJobByIdQuery request, 
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.BulkJobs
                .FindAsync(request.Id, cancellationToken);

            if (entity == null)            
                return Result<GetBulkJobByIdQueryResponse>.Fail(_localizer[Constants.Messages.NotFound]);

            var dto = new GetBulkJobByIdQueryResponse()
            {
                Id = entity.Id,
                Status = entity.Status,
                Error = entity.Error,
                Processed = entity.Processed,
                Failed = entity.Failed
            };

            return Result<GetBulkJobByIdQueryResponse>.Success(dto);
        }
    }

    public class GetBulkJobByIdQueryResponse
    {
        public string Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public int Processed { get; set; }
        public int Failed { get; set; }
    }
}
