using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Requests;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace ApplicationCore.Features.Projects.Queries
{
    public class GetPaginatedProjectsQuery : PaginateRequest,
        IRequest<PaginatedResult<GetPaginatedProjectsQueryResponse>>
    {
        public string CustomerId { get; set; }
        public string ContactId { get; set; }
        public string SearchString { get; set; }

        public string ExternalId { get; set; }

        public GetPaginatedProjectsQuery(
            int pageNumber,
            int pageSize,
            string orderBy,
            string customerId,
            string contactId,
            string searchString,
            string externalId)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            OrderBy = orderBy;
            CustomerId = customerId;
            ContactId = contactId;
            SearchString = searchString;
            ExternalId = externalId;
        }
    }

    internal class GetPaginatedProjectsQueryHandler :
        IRequestHandler<GetPaginatedProjectsQuery, PaginatedResult<GetPaginatedProjectsQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetPaginatedProjectsQueryHandler(
            ILogger<GetPaginatedProjectsQueryHandler> logger,
            IUserSessionService userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetPaginatedProjectsQueryHandler> localizer,
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

        public async Task<PaginatedResult<GetPaginatedProjectsQueryResponse>> Handle(
            GetPaginatedProjectsQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var validator = new GetPaginatedProjectsQueryValidator(_localizer);

                var valResult = validator.Validate(request);

                if (!valResult.IsValid)
                    return (PaginatedResult<GetPaginatedProjectsQueryResponse>)Result.Fail(valResult.Errors.Select(_ => _.ErrorMessage));


#pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(
                    $"GetPaginatedProjectsQuery:{JsonSerializer.Serialize(request)}",
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

                        var query = _dbContext.Projects
                            .AsNoTracking()
                            .Include(_ => _.Customer)
                            .AsQueryable();

                        if (!string.IsNullOrEmpty(request.CustomerId)
                            && !string.IsNullOrWhiteSpace(request.CustomerId))
                        {
                            query = query
                                .Where(_ => _.CustomerId == request.CustomerId);
                        }

                        if (!string.IsNullOrEmpty(request.ContactId)
                            && !string.IsNullOrWhiteSpace(request.ContactId))
                        {
                            query = query
                                .Where(_ => _.ContactId == request.ContactId);
                        }

                        if (!string.IsNullOrEmpty(request.SearchString)
                            && !string.IsNullOrWhiteSpace(request.SearchString))
                        {
                            query = query
                                .Where(_ => _.SearchVector.Matches(request.SearchString));
                        }

                        if (!string.IsNullOrEmpty(request.ExternalId)
                            && !string.IsNullOrWhiteSpace(request.ExternalId))
                        {
                            query = query
                                .Where(_ => _.ExternalId == request.ExternalId);
                        }

                        query = query.OrderBy($"{request.OrderBy} {sortDir}");

                        int total = await query.CountAsync(cancellationToken);

                        int skip = (request.PageNumber - 1) * request.PageSize;

                        var entities = await query
                            .Take(request.PageSize)
                            .Skip(skip)
                            .ToArrayAsync(cancellationToken);

                        var dtos = entities == null ? Enumerable.Empty<GetPaginatedProjectsQueryResponse>()
                            : _mapper.Map<Project[], GetPaginatedProjectsQueryResponse[]>(entities);

                        return new PaginatedResult<GetPaginatedProjectsQueryResponse>(
                            true,
                            dtos,
                            Enumerable.Empty<string>(),
                            total,
                            request.PageNumber,
                            request.PageSize);
                    });
#pragma warning restore CS8603 // Possible null reference return.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return PaginatedResult<GetPaginatedProjectsQueryResponse>.Failure(_localizer["Internal Error"]);
        }
    }

    public class GetPaginatedProjectsQueryResponse : BaseResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public Instant DateStart { get; set; }
        public Instant DateDue { get; set; }

        public string CustomerName { get; set; }
    }

    public class GetPaginatedProjectsQueryProfile : Profile
    {
        public GetPaginatedProjectsQueryProfile()
        {
            CreateMap<Project, GetPaginatedProjectsQueryResponse>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
                .IncludeBase<AuditableEntity<string>, BaseResponse>();
        }
    }

    public class GetPaginatedProjectsQueryValidator : AbstractValidator<GetPaginatedProjectsQuery>
    {
        readonly IStringLocalizer _localizer;

        public GetPaginatedProjectsQueryValidator(
            IStringLocalizer localizer)
        {
            _localizer = localizer;

            RuleFor(_ => _.PageNumber)
                .GreaterThan(0)
                .WithMessage(_localizer[Const.Messages.PageNumberGreaterThanZero]);
            RuleFor(_ => _.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage(_localizer[Const.Messages.PageSizeBetweenOneAndOneHundred]);
        }
    }
}
