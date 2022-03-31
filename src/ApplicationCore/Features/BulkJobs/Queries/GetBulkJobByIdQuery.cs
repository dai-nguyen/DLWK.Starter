using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public GetBulkJobByIdQueryHandler(
            ILogger<GetBulkJobByIdQueryHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<GetBulkJobByIdQueryHandler> localizer,            
            IMemoryCache cache)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;            
            _cache = cache;
        }

        public Task<Result<GetBulkJobByIdQueryResponse>> Handle(
            GetBulkJobByIdQuery request, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class GetBulkJobByIdQueryResponse
    {
        
    }
}
