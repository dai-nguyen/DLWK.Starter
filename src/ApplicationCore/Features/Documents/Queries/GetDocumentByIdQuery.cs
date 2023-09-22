using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Documents.Queries
{
    public class GetDocumentByIdQuery : IRequest<Result<GetDocumentByIdQueryResponse>>
    {
        public string Id { get; set; }
    }

    internal class GetDocumentByIdQueryHandler : 
        IRequestHandler<GetDocumentByIdQuery, Result<GetDocumentByIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;

        readonly IMapper _mapper;

        public GetDocumentByIdQueryHandler(
            ILogger<GetDocumentByIdQueryHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<GetDocumentByIdQueryHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Result<GetDocumentByIdQueryResponse>> Handle(
            GetDocumentByIdQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Documents.FindAsync(request.Id, cancellationToken);

                if (entity == null)
                {
                    return Result<GetDocumentByIdQueryResponse>.Fail(_localizer["Document Not Found"]);
                }

                return Result<GetDocumentByIdQueryResponse>.Success(_mapper.Map<GetDocumentByIdQueryResponse>(entity));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return Result<GetDocumentByIdQueryResponse>.Fail(new string[] { _localizer["Internal Error"] });
        }
    }

    public class GetDocumentByIdQueryResponse : BaseResponse
    {        
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public string URL { get; set; }
        public string DocumentType { get; set; }
        public string DocumentTypeId { get; set; }
    }

    public class GetDocumentByIdQueryProfile : Profile
    {
        public GetDocumentByIdQueryProfile()
        {
            CreateMap<Document, GetDocumentByIdQueryResponse>();
        }
    }    
}
