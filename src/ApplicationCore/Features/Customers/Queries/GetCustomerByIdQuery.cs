using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Features.Contacts.Queries;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ApplicationCore.Features.Customers.Queries
{
    public class GetCustomerByIdQuery : IRequest<Result<GetCustomerByIdQueryResponse>>
    {
        public string Id { get; set; }
    }

    internal class GetCustomerByIdQueryHandler :
        IRequestHandler<GetCustomerByIdQuery, Result<GetCustomerByIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetCustomerByIdQueryHandler(
            ILogger<GetCustomerByIdQueryHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<GetCustomerByIdQueryHandler> localizer,
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

        public async Task<Result<GetCustomerByIdQueryResponse>> Handle(
            GetCustomerByIdQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
#pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(
                    $"GetCustomerByIdQuery:{JsonSerializer.Serialize(request)}",
                    async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);

                        var entity = await _dbContext.Customers.FindAsync(request.Id, cancellationToken);

                        if (entity == null)
                        {
                            return Result<GetCustomerByIdQueryResponse>.Fail(_localizer[Const.Messages.NotFound]);
                        }

                        return Result<GetCustomerByIdQueryResponse>.Success(_mapper.Map<GetCustomerByIdQueryResponse>(entity));
                    });
#pragma warning restore CS8603 // Possible null reference return.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return Result<GetCustomerByIdQueryResponse>.Fail(new string[] { _localizer[Const.Messages.InternalError] });
        }
    }

    public class GetCustomerByIdQueryResponse : ResponseBase
    {        
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Industries { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }

    public class GetCustomerByIdQueryProfile : Profile
    {
        public GetCustomerByIdQueryProfile()
        {
            CreateMap<Customer, GetCustomerByIdQueryResponse>();
        }
    }
}
