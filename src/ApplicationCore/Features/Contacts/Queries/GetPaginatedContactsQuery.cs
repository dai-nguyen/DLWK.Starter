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
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace ApplicationCore.Features.Contacts.Queries
{
    public class GetPaginatedContactsQuery : PaginateRequest, IRequest<PaginatedResult<GetPaginatedContactsQueryResponse>>
    {
        public string CustomerId { get; set; }
        public string SearchString { get; set; }

        public GetPaginatedContactsQuery(
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

    internal class GetPaginatedContactsQueryHandler :
        IRequestHandler<GetPaginatedContactsQuery, PaginatedResult<GetPaginatedContactsQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetPaginatedContactsQueryHandler(
            ILogger<GetPaginatedContactsQueryHandler> logger,
            IUserSessionService userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetPaginatedContactsQueryHandler> localizer,
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

        public async Task<PaginatedResult<GetPaginatedContactsQueryResponse>> Handle(
            GetPaginatedContactsQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
#pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(
                    $"GetPaginatedContactsQuery:{JsonSerializer.Serialize(request)}",
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

                        var query = _dbContext.Contacts
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

                        var dtos = entities == null ? Enumerable.Empty<GetPaginatedContactsQueryResponse>()
                            : _mapper.Map<Contact[], GetPaginatedContactsQueryResponse[]>(entities);
                        
                        return new PaginatedResult<GetPaginatedContactsQueryResponse>(
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
                _logger.LogError(ex, "Error getting paginated contacts {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return PaginatedResult<GetPaginatedContactsQueryResponse>.Failure(_localizer["Internal Error"]);
        }
    }

    public class GetPaginatedContactsQueryResponse
    {
        public string Id { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class GetPaginatedContactsQueryProfile : Profile
    {
        public GetPaginatedContactsQueryProfile()
        {
            CreateMap<Contact, GetPaginatedContactsQueryResponse>();
        }
    }
}
