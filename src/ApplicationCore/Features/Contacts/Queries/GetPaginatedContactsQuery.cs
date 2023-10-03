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
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace ApplicationCore.Features.Contacts.Queries
{
    public class GetPaginatedContactsQuery : PaginateRequest, 
        IRequest<PaginatedResult<GetPaginatedContactsQueryResponse>>
    {
        public string CustomerId { get; set; }
        public string SearchString { get; set; }

        public string ExternalId { get; set; }

        public GetPaginatedContactsQuery(
            int pageNumber,
            int pageSize,
            string orderBy,
            string customerId,
            string searchString,
            string externalId)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            OrderBy = orderBy;
            CustomerId = customerId;
            SearchString = searchString;
            ExternalId = externalId;
        }
    }

    internal class GetPaginatedContactsQueryHandler :
        IRequestHandler<GetPaginatedContactsQuery, PaginatedResult<GetPaginatedContactsQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IMemoryCache _cache;

        public GetPaginatedContactsQueryHandler(
            ILogger<GetPaginatedContactsQueryHandler> logger,
            IUserSessionService userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetPaginatedContactsQueryHandler> localizer,
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

        public async Task<PaginatedResult<GetPaginatedContactsQueryResponse>> Handle(
            GetPaginatedContactsQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var validator = new GetPaginatedContactsQueryValidator(_localizer);

                var valResult = validator.Validate(request);

                if (!valResult.IsValid)
                    return (PaginatedResult<GetPaginatedContactsQueryResponse>)Result.Fail(valResult.Errors.Select(_ => _.ErrorMessage));


#pragma warning disable CS8603 // Possible null reference return.
                return await _cache.GetOrCreateAsync(
                    $"GetPaginatedContactsQuery:{JsonSerializer.Serialize(request)}",
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

                        var query = _dbContext.Contacts
                            .AsNoTracking()
                            //.Where(_ => _.CustomerId == request.CustomerId)
                            .AsQueryable();

                        if (!string.IsNullOrEmpty(request.CustomerId)
                            && !string.IsNullOrWhiteSpace(request.CustomerId))
                        {
                            query = query
                                .Where(_ => _.CustomerId ==  request.CustomerId);
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

                        var dtos = entities == null ? Enumerable.Empty<GetPaginatedContactsQueryResponse>()
                            : _mapper.Map<Contact[], GetPaginatedContactsQueryResponse[]>(entities);
                        
                        return new PaginatedResult<GetPaginatedContactsQueryResponse>(
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

            return PaginatedResult<GetPaginatedContactsQueryResponse>.Failure(_localizer["Internal Error"]);
        }
    }

    public class GetPaginatedContactsQueryResponse : ResponseBase
    {        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class GetPaginatedContactsQueryProfile : Profile
    {
        public GetPaginatedContactsQueryProfile()
        {
            CreateMap<Contact, GetPaginatedContactsQueryResponse>()
                .IncludeBase<AuditableEntity<string>, ResponseBase>();
        }
    }

    public class GetPaginatedContactsQueryValidator : AbstractValidator<GetPaginatedContactsQuery>
    {
        readonly IStringLocalizer _localizer;

        public GetPaginatedContactsQueryValidator(
            IStringLocalizer localizer)
        {
            _localizer = localizer;

            //RuleFor(_ => _.CustomerId)
            //    .NotEmpty().WithMessage(_localizer["CustomerId is required"]);

            RuleFor(_ => _.PageNumber)
                .GreaterThan(0)
                .WithMessage(_localizer[Const.Messages.PageNumberGreaterThanZero]);
            RuleFor(_ => _.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage(_localizer[Const.Messages.PageSizeBetweenOneAndOneHundred]);
        }
    }
}
