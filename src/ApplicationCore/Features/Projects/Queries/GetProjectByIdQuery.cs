using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ApplicationCore.Features.Projects.Queries
{
    public class GetProjectByIdQuery : IRequest<Result<GetProjectByIdQueryResponse>>
    {
        public string Id { get; set; }
    }

    internal class GetProjectByIdQueryHandler :
        IRequestHandler<GetProjectByIdQuery, Result<GetProjectByIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetProjectByIdQueryHandler(
            ILogger<GetProjectByIdQueryHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<GetProjectByIdQueryHandler> localizer,
            AppDbContext dbContext,
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

        public async Task<Result<GetProjectByIdQueryResponse>> Handle(
            GetProjectByIdQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
#pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(
                    $"GetProjectByIdQuery:{JsonSerializer.Serialize(request)}",
                    async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                        var entity = await _dbContext.Projects.FindAsync(request.Id, cancellationToken);

                        if (entity == null)
                        {
                            return Result<GetProjectByIdQueryResponse>.Fail(_localizer[Const.Messages.NotFound]);
                        }

                        return Result<GetProjectByIdQueryResponse>.Success(_mapper.Map<GetProjectByIdQueryResponse>(entity));
                    });
#pragma warning restore CS8603 // Possible null reference return.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return Result<GetProjectByIdQueryResponse>.Fail(new string[] { _localizer[Const.Messages.InternalError] });
        }
    }

    public class GetProjectByIdQueryResponse : ResponseBase
    {        
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateDue { get; set; }
    }

    public class GetProjectByIdQueryProfile : Profile
    {
        public GetProjectByIdQueryProfile()
        {
            CreateMap<Project, GetProjectByIdQueryResponse>()
                .IncludeBase<AuditableEntity<string>, ResponseBase>();
        }
    }
}
