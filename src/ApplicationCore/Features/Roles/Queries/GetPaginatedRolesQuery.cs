using ApplicationCore.Data;
using ApplicationCore.Helpers;
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

namespace ApplicationCore.Features.Roles.Queries
{
    public class GetPaginatedRolesQuery : PaginateRequest, IRequest<PaginatedResult<GetPaginatedRolesQueryResponse>>
    {
        public string SearchString { get; set; }

        public GetPaginatedRolesQuery(
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

    internal class GetPaginatedRolesQueryHandler :
        IRequestHandler<GetPaginatedRolesQuery, PaginatedResult<GetPaginatedRolesQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetPaginatedRolesQueryHandler(
            ILogger<GetPaginatedRolesQueryHandler> logger,
            IUserSessionService userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetPaginatedRolesQueryHandler> localizer,
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

        public async Task<PaginatedResult<GetPaginatedRolesQueryResponse>> Handle(
            GetPaginatedRolesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var permission = _userSession.Claims.GetPermission(Constants.ClaimNames.roles);

                if (!permission.can_read)
                    PaginatedResult<GetPaginatedRolesQueryResponse>.Failure(_localizer[Constants.Messages.PermissionDenied]);

                return await _cache.GetOrCreateAsync(
                    $"GetPaginatedRolesQuery:{JsonSerializer.Serialize(request)}",
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

                        var query = _dbContext.Roles
                            .AsNoTracking()
                            .AsQueryable();

                        if (!string.IsNullOrEmpty(request.SearchString)
                            && !string.IsNullOrWhiteSpace(request.SearchString))
                        {
                            query = query
                                .Where(_ => _.SearchVector.Matches(request.SearchString));
                        }

                        query = query.OrderBy($"{request.OrderBy} {sortDir}");

                        int total = await query.CountAsync();

                        int skip = (request.PageNumber - 1) * request.PageSize;

                        var data = await query
                            .Take(request.PageSize)
                            .Skip(skip)
                            .ToArrayAsync();

                        var dtos = _mapper.Map<IEnumerable<GetPaginatedRolesQueryResponse>>(data);

                        return new PaginatedResult<GetPaginatedRolesQueryResponse>(
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
                _logger.LogError(ex, "Error getting paginated roles {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return PaginatedResult<GetPaginatedRolesQueryResponse>
                .Failure(_localizer[Constants.Messages.InternalError]);
        }
    }


    public class GetPaginatedRolesQueryResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class GetPaginatedRolesQueryResponseProfile : Profile
    {
        public GetPaginatedRolesQueryResponseProfile()
        {
            CreateMap<AppRole, GetPaginatedRolesQueryResponse>();
        }
    }
}
