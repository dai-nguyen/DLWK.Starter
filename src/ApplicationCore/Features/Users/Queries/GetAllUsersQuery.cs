using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Requests;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace ApplicationCore.Features.Users.Queries
{
    public class GetAllUsersQuery : PaginateRequest, IRequest<PaginatedResult<GetAllUsersQueryResponse>>
    {
        public string SearchString { get; set; }

        public GetAllUsersQuery(
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

    internal class GetAllUsersQueryHandler :
        IRequestHandler<GetAllUsersQuery, PaginatedResult<GetAllUsersQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;

        public GetAllUsersQueryHandler(
            ILogger<GetAllUsersQueryHandler> logger,
            IUserSessionService userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetAllUsersQueryHandler> localizer,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<GetAllUsersQueryResponse>> Handle(
            GetAllUsersQuery request,
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

                var dtos = _mapper.Map<IEnumerable<GetAllUsersQueryResponse>>(data);

                return new PaginatedResult<GetAllUsersQueryResponse>(
                    true,
                    dtos,
                    Enumerable.Empty<string>(),
                    total,
                    request.PageNumber,
                    request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated users {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return PaginatedResult<GetAllUsersQueryResponse>.Failure(_localizer["Internal Error"]);
        }
    }

    public class GetAllUsersQueryResponse
    {
        public string Id { get; set; }
        public virtual string ExternalId { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class GetAllUsersQueryProfile : Profile
    {
        public GetAllUsersQueryProfile()
        {
            CreateMap<AppUser, GetAllUsersQueryResponse>();
        }
    }
}
