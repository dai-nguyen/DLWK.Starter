using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Contacts.Queries
{
    public class GetContactByIdQuery : IRequest<Result<GetContactByIdQueryResponse>>
    {
        public string Id { get; set; }
    }

    internal class GetContactByIdQueryHandler :
        IRequestHandler<GetContactByIdQuery, Result<GetContactByIdQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;

        readonly IMapper _mapper;

        public GetContactByIdQueryHandler(
            ILogger<GetContactByIdQueryHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<GetContactByIdQueryHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Result<GetContactByIdQueryResponse>> Handle(
            GetContactByIdQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Contacts.FindAsync(request.Id, cancellationToken);

                if (entity == null)
                {
                    return Result<GetContactByIdQueryResponse>.Fail(_localizer["Contact Not Found"]);
                }

                return Result<GetContactByIdQueryResponse>.Success(_mapper.Map<GetContactByIdQueryResponse>(entity));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact {@0} {UserId}",
                    request, _userSession.UserId);
            }

            return Result<GetContactByIdQueryResponse>.Fail(new string[] { _localizer["Internal Error"] });
        }
    }

    public class GetContactByIdQueryResponse
    {
        public string Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string ExternalId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string CustomerId { get; set; }
    }

    public class GetContactByIdQueryProfile : Profile
    {
        public GetContactByIdQueryProfile() 
        {
            CreateMap<Contact, GetContactByIdQueryResponse>();
        }
    }
}
