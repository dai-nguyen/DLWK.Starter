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
    public class GetBanksByOwnerIdQuery : IRequest<Result<GetBankByOwnerIdQueryResponse>>
    {
        public string OwnerId { get; set; } = string.Empty;
    }

    internal class GetBankByOwnerIdQueryHandler : IRequestHandler<GetBanksByOwnerIdQuery, Result<GetBankByOwnerIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;
        readonly PointRewardModuleDbContext _dbContext;

        public GetBankByOwnerIdQueryHandler(
            ILogger<GetBankByOwnerIdQueryHandler> logger,
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

        public async Task<Result<GetBankByOwnerIdQueryResponse>> Handle(
            GetBanksByOwnerIdQuery query,
            CancellationToken cancellationToken)
        {            
            return await _cache.GetOrCreateAsync(
                $"GetBankByOwnerIdQuery:{JsonSerializer.Serialize(query)}",
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                    var entity = await _dbContext.Banks
                        .FirstOrDefaultAsync(_ => _.OwnerId == query.OwnerId, cancellationToken);

                    if (entity == null)
                        return Result<GetBankByOwnerIdQueryResponse>.Fail(_localizer[Constants.Messages.NotFound]);

                    return Result<GetBankByOwnerIdQueryResponse>.Success(_mapper.Map<GetBankByOwnerIdQueryResponse>(entity));
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
