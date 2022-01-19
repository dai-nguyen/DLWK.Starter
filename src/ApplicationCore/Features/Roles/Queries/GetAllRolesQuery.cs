using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Roles.Queries
{
    public class GetAllRolesQuery : IRequest<Result<IEnumerable<GetAllRolesQueryResponse>>>
    {
        public string SearchString { get; set; }
    }

    internal class GetAllRolesQueryHandler :
        IRequestHandler<GetAllRolesQuery, Result<IEnumerable<GetAllRolesQueryResponse>>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;
        
        public GetAllRolesQueryHandler(
            ILogger<GetAllRolesQueryHandler> logger,
            IUserSessionService userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetAllRolesQueryHandler> localizer,
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

        public async Task<Result<IEnumerable<GetAllRolesQueryResponse>>> Handle(
            GetAllRolesQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _cache.GetOrCreateAsync(
                    $"GetAllRolesQuery:{request.SearchString ?? ""}",
                    async entry => 
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);

                        var query = _dbContext.Roles
                            .AsNoTracking()
                            .AsQueryable();

                        if (!string.IsNullOrWhiteSpace(request.SearchString))
                            query = query.Where(_ => _.SearchVector.Matches(request.SearchString));

                        var entities = await query.OrderBy(_ => _.Name).ToArrayAsync();

                        var roles = _mapper.Map<IEnumerable<GetAllRolesQueryResponse>>(entities);

                        return Result<IEnumerable<GetAllRolesQueryResponse>>.Success(roles);
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated roles {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return Result<IEnumerable<GetAllRolesQueryResponse>>.Fail();
        }
    }

    public class GetAllRolesQueryResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class GetAllRolesQueryProfile : Profile
    {
        public GetAllRolesQueryProfile()
        {
            CreateMap<AppRole, GetAllRolesQueryResponse>();
        }
    }
}
