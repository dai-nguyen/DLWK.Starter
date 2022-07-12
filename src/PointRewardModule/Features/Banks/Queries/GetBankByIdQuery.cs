using ApplicationCore;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PointRewardModule.Data;
using System.Text.Json;

namespace PointRewardModule.Features.Banks.Queries
{
    public class GetBankByIdQuery : IRequest<Result<GetBankByIdQueryResponse>>
    {
        public string Id { get; set; } = string.Empty;
    }

    internal class GetBankByIdQueryHandler : IRequestHandler<GetBankByIdQuery, Result<GetBankByIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly IMemoryCache _cache;
        readonly PointRewardModuleDbContext _dbContext;

        public GetBankByIdQueryHandler(
            ILogger<GetBankByIdQueryHandler> logger, 
            IUserSessionService userSession, 
            IStringLocalizer localizer, 
            IMemoryCache cache, 
            PointRewardModuleDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _cache = cache;
            _dbContext = dbContext;
        }

        public async Task<Result<GetBankByIdQueryResponse>> Handle(
            GetBankByIdQuery query,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Banks.FindAsync(query.Id);

            if (entity == null)
                return Result<GetBankByIdQueryResponse>.Fail(_localizer[Constants.Messages.NotFound]);

            return await _cache.GetOrCreateAsync(
                $"GetBankByIdQuery:{JsonSerializer.Serialize(query)}",
                async entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                    var enitty = await _dbContext.Banks.FindAsync(query.Id);

                    if ()
                });
        }
    }

    public class GetBankByIdQueryResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Balance { get; set; }
    }
}
