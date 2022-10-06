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

namespace ApplicationCore.Features.Documents.Queries
{
    public class GetPaginatedDocumentsQuery : PaginateRequest, IRequest<PaginatedResult<GetPaginatedDocumentsQueryResponse>>
    {
        public string SearchString { get; set; }

        public GetPaginatedDocumentsQuery(
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

    internal class GetPaginatedDocumentsQueryHandler : 
        IRequestHandler<GetPaginatedDocumentsQuery, PaginatedResult<GetPaginatedDocumentsQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;        
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetPaginatedDocumentsQueryHandler(
            ILogger<GetPaginatedDocumentsQueryHandler> logger,
            IUserSessionService userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetPaginatedDocumentsQueryHandler> localizer,
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

        public async Task<PaginatedResult<GetPaginatedDocumentsQueryResponse>> Handle(
            GetPaginatedDocumentsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _cache.GetOrCreateAsync(
                    $"GetPaginatedDocumentsQuery:{JsonSerializer.Serialize(request)}",
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

                        var query = _dbContext.Documents                            
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

                        var dtos = await query                            
                            .Select(_ => new GetPaginatedDocumentsQueryResponse
                            {
                                Id = _.Id,
                                DateCreated = _.DateCreated,
                                DateUpdated = _.DateUpdated,
                                CreatedBy = _.CreatedBy,
                                UpdatedBy = _.UpdatedBy,
                                ExternalId = _.ExternalId,
                                Title = _.Title,
                                Description = _.Description,
                                Size = _.Size,
                                ContentType = _.ContentType
                            })
                            .Take(request.PageSize)
                            .Skip(skip)
                            .ToArrayAsync(cancellationToken);                        

                        return new PaginatedResult<GetPaginatedDocumentsQueryResponse>(
                            true,
                            dtos,
                            Enumerable.Empty<string>(),
                            total,
                            request.PageNumber,
                            request.PageSize);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated documents {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return PaginatedResult<GetPaginatedDocumentsQueryResponse>.Failure(_localizer["Internal Error"]);
        }
    }

    public class GetPaginatedDocumentsQueryResponse
    {
        public string Id { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    public class GetAllDocumentsQueryProfile : Profile
    {
        public GetAllDocumentsQueryProfile()
        {
            CreateMap<Document, GetPaginatedDocumentsQueryResponse>();
        }
    }
    
}
