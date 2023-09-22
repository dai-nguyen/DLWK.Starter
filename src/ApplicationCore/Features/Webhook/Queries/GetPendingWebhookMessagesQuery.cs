using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Features.Customers.Queries;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Webhook.Queries
{
    public class GetPendingWebhookMessagesQuery : IRequest<Result<IEnumerable<GetPendingWebhookMessagesQueryResponse>>>
    {
        
    }

    internal class GetPendingWebhookMessagesQueryHandler :
        IRequestHandler<GetPendingWebhookMessagesQuery, Result<IEnumerable<GetPendingWebhookMessagesQueryResponse>>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;

        public GetPendingWebhookMessagesQueryHandler(
            ILogger<GetPendingWebhookMessagesQuery> logger,
            IUserSessionService userSession,
            IStringLocalizer<GetPaginatedCustomersQueryHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;            
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<GetPendingWebhookMessagesQueryResponse>>> Handle(
            GetPendingWebhookMessagesQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entities = await _dbContext.WebhookMessages
                    .AsTracking()
                    .Include(_ => _.Subscriber)
                    .Where(_ => _.Subscriber.IsEnabled == true
                        && (_.IsOkResponse == null || _.IsOkResponse == false))
                    .ToArrayAsync();

                var dtos = entities == null ? Enumerable.Empty<GetPendingWebhookMessagesQueryResponse>() :
                    _mapper.Map<WebhookMessage[], GetPendingWebhookMessagesQueryResponse[]>(entities);

                return Result<IEnumerable<GetPendingWebhookMessagesQueryResponse>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {@0} {UserId}",
                    request, _userSession.UserId);
            }
            return Result<IEnumerable<GetPendingWebhookMessagesQueryResponse>>
                .Fail(_localizer["Internal Error"]);
        }
    }

    public class GetPendingWebhookMessagesQueryResponse
    {
        public string MessageId { get; set; } = string.Empty;
        public string SubscriberId { get; set; }

        public string EntityName { get; set; }
        public string Operation { get; set; }
        public string Url { get; set; }        
        public int FailedCount { get; set; }

        public string EntityId { get; set; }                
        public bool? IsOkResponse { get; set; }
    }

    public class GetPendingWebhookMessageQueryProfile : Profile
    {
        public GetPendingWebhookMessageQueryProfile() 
        { 
            CreateMap<WebhookMessage, GetPendingWebhookMessagesQueryResponse>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SubscriberId, opt => opt.MapFrom(src => src.SubscriberId))
                .ForMember(dest => dest.EntityName, opt => opt.MapFrom(src => src.Subscriber.EntityName))
                .ForMember(dest => dest.Operation, opt => opt.MapFrom(src => src.Subscriber.Operation))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Subscriber.Url))
                .ForMember(dest => dest.FailedCount, opt => opt.MapFrom(src => src.Subscriber.FailedCount))
                .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId))
                .ForMember(dest => dest.IsOkResponse, opt => opt.MapFrom(src => src.IsOkResponse));
        }

    }
}
