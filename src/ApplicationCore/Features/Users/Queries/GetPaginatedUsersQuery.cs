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

namespace ApplicationCore.Features.Users.Queries
{
    public class GetPaginatedUsersQuery : PaginateRequest, IRequest<PaginatedResult<GetPaginatedUsersQueryResponse>>
    {
        public string SearchString { get; set; }

        public GetPaginatedUsersQuery(
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

    internal class GetPaginatedUsersQueryHandler :
        IRequestHandler<GetPaginatedUsersQuery, PaginatedResult<GetPaginatedUsersQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetPaginatedUsersQueryHandler(
            ILogger<GetPaginatedUsersQueryHandler> logger,
            IUserSessionService userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetPaginatedUsersQueryHandler> localizer,
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

        public async Task<PaginatedResult<GetPaginatedUsersQueryResponse>> Handle(
            GetPaginatedUsersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var permission = _userSession.Claims.GetPermission(Constants.ClaimNames.users);

                if (!permission.can_read)
                    PaginatedResult<GetPaginatedUsersQueryResponse>.Failure(_localizer[Constants.Messages.PermissionDenied]);

                return await _cache.GetOrCreateAsync(
                    $"GetPaginatedUsersQuery:{JsonSerializer.Serialize(request)}",
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

                        var query = _dbContext.Users
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

                        var dtos = _mapper.Map<IEnumerable<GetPaginatedUsersQueryResponse>>(data);

                        return new PaginatedResult<GetPaginatedUsersQueryResponse>(
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
                _logger.LogError(ex, "Error getting paginated users {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return PaginatedResult<GetPaginatedUsersQueryResponse>.Failure(_localizer[Constants.Messages.InternalError]);
        }
    }

    public class GetPaginatedUsersQueryResponse
    {
        public string Id { get; set; } = string.Empty;
        public virtual string ExternalId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        public string ProfilePicture { get; set; } = string.Empty;
    }

    public class GetAllUsersQueryProfile : Profile
    {
        public GetAllUsersQueryProfile()
        {
            CreateMap<AppUser, GetPaginatedUsersQueryResponse>();
        }
    }
}
