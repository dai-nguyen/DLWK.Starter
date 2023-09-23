using ApplicationCore.Constants;
using ApplicationCore.Constants.Constants;
using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Contacts.Commands
{
    public class UpdateContactCommand : BaseUpdateRequest, IRequest<Result<string>>
    {        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    internal class UpdateContactCommandHandler :
        IRequestHandler<UpdateContactCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;

        public UpdateContactCommandHandler(
            ILogger<UpdateContactCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateContactCommandHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Result<string>> Handle(
            UpdateContactCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Contacts.FindAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer[Const.Messages.NotFound]);
                }

                _mapper.Map(command, entity);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(entity.Id,
                    _localizer[Const.Messages.Saved]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {@0} {UserId}",
                    command, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer[Const.Messages.InternalError]);
        }
    }

    public class UpdateContactCommandValidator : AbstractValidator<UpdateContactCommand>
    {
        readonly IStringLocalizer _localizer;

        public UpdateContactCommandValidator(
            IStringLocalizer<UpdateContactCommandValidator> localizer)
        {
            _localizer = localizer;

            RuleFor(_ => _.FirstName)
                .NotEmpty().WithMessage(_localizer["FirstName is required"])
                .MaximumLength(ContactConst.FirstNameMaxLength)
                .WithMessage(_localizer[$"FirstName cannot be longer than {ContactConst.FirstNameMaxLength}"]);

            RuleFor(_ => _.LastName)
                .NotEmpty().WithMessage(_localizer["LastName is required"])
                .MaximumLength(ContactConst.LastNameMaxLength)
                .WithMessage(_localizer[$"LastName cannot be longer than {ContactConst.LastNameMaxLength}"]);

            RuleFor(_ => _.Email)
                .NotEmpty().WithMessage(_localizer["Email is required"])
                .MaximumLength(ContactConst.EmailMaxLength)
                .WithMessage(_localizer[$"Email cannot be longer than {ContactConst.EmailMaxLength}"]);

            RuleFor(_ => _.Phone)
                .MaximumLength(ContactConst.PhoneMaxLength)
                .WithMessage(_localizer[$"Phone cannot be longer than {ContactConst.PhoneMaxLength}"]);

            RuleFor(_ => _.Id)
                .NotEmpty().WithMessage(_localizer["Contact is required"]);
        }
    }

    public class UpdateContactCommandProfile : Profile
    {
        public UpdateContactCommandProfile()
        {
            CreateMap<UpdateContactCommand, Contact>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .IncludeBase<BaseUpdateRequest, AuditableEntity<string>>();
        }
    }
}
