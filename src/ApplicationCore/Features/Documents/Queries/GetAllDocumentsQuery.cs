using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Requests;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;

namespace ApplicationCore.Features.Documents.Queries
{
    public class GetAllDocumentsQuery : PaginateRequest, IRequest<PaginatedResult<GetAllDocumentsQueryResponse>>
    {
        public string SearchString { get; set; }

        public GetAllDocumentsQuery(
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

    internal class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, PaginatedResult<GetAllDocumentsQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSession _userSession;
        readonly IStringLocalizer<GetAllDocumentsQueryHandler> _localizer;
        readonly AppDbContext _dbContext;        
        readonly IMapper _mapper;        

        public GetAllDocumentsQueryHandler(
            ILogger<GetAllDocumentsQueryHandler> logger,
            IUserSession userSession,
            AppDbContext dbContext,
            IStringLocalizer<GetAllDocumentsQueryHandler> localizer,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;            
            _mapper = mapper;            
        }

        public async Task<PaginatedResult<GetAllDocumentsQueryResponse>> Handle(
            GetAllDocumentsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var sortDir = request.IsDescending ? "desc" : "asc";
                
                if (string.IsNullOrEmpty(request.OrderBy))
                    request.OrderBy = "Id";

                var query = _dbContext.Documents
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

                var data = query
                    .Take(request.PageSize)
                    .Skip(skip)
                    .ToArrayAsync();

                var dtos = _mapper.Map<IEnumerable<GetAllDocumentsQueryResponse>>(data);

                return new PaginatedResult<GetAllDocumentsQueryResponse>(
                    true,
                    dtos,
                    Enumerable.Empty<string>(),
                    total,
                    request.PageNumber,
                    request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated documents {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return PaginatedResult<GetAllDocumentsQueryResponse>.Failure(new string[] { _localizer["Internal Error"] });
        }
    }

    public class GetAllDocumentsQueryResponse
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string ExternalId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public string URL { get; set; }
        public string DocumentType { get; set; }
        public string DocumentTypeId { get; set; }
    }

    public class GetAllDocumentsQueryProfile : Profile
    {
        public GetAllDocumentsQueryProfile()
        {
            CreateMap<Document, GetAllDocumentsQueryResponse>();
        }
    }
    
}
