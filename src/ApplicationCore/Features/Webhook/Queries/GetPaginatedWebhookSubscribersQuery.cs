using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Features.Customers.Queries;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Requests;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace ApplicationCore.Features.Webhook.Queries
{
    public class GetPaginatedWebhookSubscribersQuery : PaginateRequest, IRequest<PaginatedResult<GetPaginatedWebhookSubscribersQueryResponse>>
    {
        public string EntityName { get; set; }
        public string Operation { get; set; }

        public GetPaginatedWebhookSubscribersQuery(
            int pageNumber,
            int pageSize,
            string orderBy,
            string entityName,
            string operation)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            OrderBy = orderBy;            
            EntityName = entityName;
            Operation = operation;
        }
    }

    internal class GetPaginatedWebhookSubscribersQueryHandler :
        IRequestHandler<GetPaginatedWebhookSubscribersQuery, PaginatedResult<GetPaginatedWebhookSubscribersQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;

        public GetPaginatedWebhookSubscribersQueryHandler(
            ILogger<GetPaginatedWebhookSubscribersQueryHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<GetPaginatedWebhookSubscribersQueryHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;            
            _mapper = mapper;
        }

        public async Task<PaginatedResult<GetPaginatedWebhookSubscribersQueryResponse>> Handle(
            GetPaginatedWebhookSubscribersQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {                
                if (request.PageNumber <= 0)
                    request.PageNumber = 1;
                if (request.PageSize <= 0)
                    request.PageSize = 15;

                var sortDir = request.IsDescending ? "desc" : "asc";

                if (string.IsNullOrEmpty(request.OrderBy))
                    request.OrderBy = "Id";

                var query = _dbContext.WebhookSubsribers
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.EntityName)
                    && !string.IsNullOrWhiteSpace(request.EntityName))
                {
                    query = query.Where(_ => _.EntityName == request.EntityName);
                }

                if (!string.IsNullOrEmpty(request.Operation)
                    && !string.IsNullOrWhiteSpace(request.Operation))
                {
                    query = query.Where(_ => _.Operation == request.Operation);
                }

                query = query.OrderBy($"{request.OrderBy} {sortDir}");

                int total = await query.CountAsync(cancellationToken);

                int skip = (request.PageNumber - 1) * request.PageSize;

                var entities = await query
                    .Take(request.PageSize)
                    .Skip(skip)
                    .ToArrayAsync(cancellationToken);

                var dtos = entities == null ? Array.Empty<GetPaginatedWebhookSubscribersQueryResponse>()
                            : _mapper.Map<WebhookSubscriber[], GetPaginatedWebhookSubscribersQueryResponse[]>(entities);

                return new PaginatedResult<GetPaginatedWebhookSubscribersQueryResponse>(
                    true,
                    dtos,
                    Enumerable.Empty<string>(),
                    total,
                    request.PageNumber,
                    request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated {@0} {UserId}",
                   request, _userSession.UserId);
            }
            return PaginatedResult<GetPaginatedWebhookSubscribersQueryResponse>
                .Failure(_localizer[Constants.Const.Messages.InternalError]);
        }
    }

    public class GetPaginatedWebhookSubscribersQueryResponse : BaseResponse
    {        
        public string EntityName { get; set; }
        public string Operation { get; set; }
        public string Url { get; set; }
        public bool IsEnabled { get; set; }
        public int FailedCount { get; set; }
    }

    public class GetPaginatedWebhookSubscribersQueryProfile : Profile
    {
        public GetPaginatedWebhookSubscribersQueryProfile() 
        { 
            CreateMap<WebhookSubscriber, GetPaginatedWebhookSubscribersQueryResponse>();
        }
    }
}
