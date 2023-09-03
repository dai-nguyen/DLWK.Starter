using ApplicationCore;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PointRewardModule.Data;
using PointRewardModule.Entities;
using System.Text.Json;

namespace PointRewardModule.Features.Banks.Queries
{
    public class GetBanksByOwnerIdQuery : IRequest<Result<IEnumerable<GetBankByOwnerIdQueryResponse>>>
    {
        public string OwnerId { get; set; } = string.Empty;
    }

    internal class GetBanksByOwnerIdQueryHandler : IRequestHandler<GetBanksByOwnerIdQuery, Result<IEnumerable<GetBankByOwnerIdQueryResponse>>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;
        readonly PointRewardModuleDbContext _dbContext;

        public GetBanksByOwnerIdQueryHandler(
            ILogger<GetBanksByOwnerIdQueryHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer localizer,
            IMapper mapper,
            IMemoryCache cache,
            PointRewardModuleDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _mapper = mapper;
            _cache = cache;
            _dbContext = dbContext;
        }

        public async Task<Result<IEnumerable<GetBankByOwnerIdQueryResponse>>> Handle(
            GetBanksByOwnerIdQuery query,
            CancellationToken cancellationToken)
        {            
            return await _cache.GetOrCreateAsync(
                $"GetBankByOwnerIdQuery:{JsonSerializer.Serialize(query)}",
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                    var entities = await _dbContext.Banks
                        .Where(_ => _.OwnerId == query.OwnerId)
                        .ToListAsync();

                    if (entities == null)
                        return Result<IEnumerable<GetBankByOwnerIdQueryResponse>>.Fail(_localizer[Constants.Messages.NotFound]);

                    var dtos = _mapper.Map<IEnumerable<GetBankByOwnerIdQueryResponse>>(entities);

                    return Result<IEnumerable<GetBankByOwnerIdQueryResponse>>
                        .Success(dtos);
                });
        }
    }

    public class GetBankByOwnerIdQueryResponse
    {
        public string Id { get; set; } = string.Empty;        
        public string BankType { get; set; } = string.Empty;
        public int Balance { get; set; }
    }

    public class GetBankByOwnerIdQueryProfile : Profile
    {
        public GetBankByOwnerIdQueryProfile()
        {
            CreateMap<Bank, GetBankByOwnerIdQueryResponse>();
        }
    }
}
