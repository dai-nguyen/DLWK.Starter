using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Requests;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace ApplicationCore.Features.Customers.Queries
{
    public class GetPaginatedCustomersQuery : PaginateRequest, IRequest<PaginatedResult<GetPaginatedCustomersQueryResponse>>
    {        
        public string SearchString { get; set; }

        public GetPaginatedCustomersQuery(
            int pageNumber,
            int pageSize,
            string orderBy,            
            string searchString)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            OrderBy = orderBy;            
            SearchString = searchString;
        }
    }

    internal class GetPaginatedCustomersQueryHandler :
        IRequestHandler<GetPaginatedCustomersQuery, PaginatedResult<GetPaginatedCustomersQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetPaginatedCustomersQueryHandler(
            ILogger<GetPaginatedCustomersQueryHandler> logger,
            IUserSessionService userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetPaginatedCustomersQueryHandler> localizer,
            IMapper mapper,
            IMemoryCache cache)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<PaginatedResult<GetPaginatedCustomersQueryResponse>> Handle(
            GetPaginatedCustomersQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
#pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(
                    $"GetPaginatedCustomersQuery:{JsonSerializer.Serialize(request)}",
                    async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                        if (request.PageNumber <= 0)
                            request.PageNumber = 1;
                        if (request.PageSize <= 0)
                            request.PageSize = 15;

                        var sortDir = request.IsDescending ? "desc" : "asc";

                        if (string.IsNullOrEmpty(request.OrderBy))
                            request.OrderBy = "Id";

                        var query = _dbContext.Customers
                            .AsNoTracking()                            
                            .AsQueryable();

                        if (!string.IsNullOrEmpty(request.SearchString)
                            && !string.IsNullOrWhiteSpace(request.SearchString))
                        {
                            query = query
                                .Where(_ => _.SearchVector.Matches(request.SearchString));
                        }

                        query = query.OrderBy($"{request.OrderBy} {sortDir}");

                        int total = await query.CountAsync(cancellationToken);

                        int skip = (request.PageNumber - 1) * request.PageSize;

                        var entities = await query
                            .Take(request.PageSize)
                            .Skip(skip)
                            .ToArrayAsync(cancellationToken);

                        var dtos = entities == null ? Enumerable.Empty<GetPaginatedCustomersQueryResponse>()
                            : _mapper.Map<Customer[], GetPaginatedCustomersQueryResponse[]>(entities);

                        return new PaginatedResult<GetPaginatedCustomersQueryResponse>(
                            true,
                            dtos,
                            Enumerable.Empty<string>(),
                            total,
                            request.PageNumber,
                            request.PageSize);
                    });
#pragma warning restore CS8603 // Possible null reference return.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return PaginatedResult<GetPaginatedCustomersQueryResponse>.Failure(_localizer["Internal Error"]);
        }
    }

    public class GetPaginatedCustomersQueryResponse
    {
        public string Id { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;

        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Industries { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }
}
