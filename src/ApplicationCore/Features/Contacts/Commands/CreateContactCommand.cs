using ApplicationCore.Constants;
using ApplicationCore.Constants.Constants;
using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Contacts.Commands
{
    public class CreateContactCommand : BaseCreateRequest, IRequest<Result<string>>
    {        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string CustomerId { get; set; }
    }

    internal class CreateContactCommandHandler :
        IRequestHandler<CreateContactCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;

        public CreateContactCommandHandler(
            ILogger<CreateContactCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateContactCommandHandler> localizer,
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
            CreateContactCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var isValidCustomer = await _dbContext.Customers.AnyAsync(_ => _.Id == command.CustomerId);

                if (!isValidCustomer)
                {
                    return Result<string>.Fail(_localizer["Invalid Customer ID"]);
                }

                var entity = _mapper.Map<Contact>(command);
                
                _dbContext.Contacts.Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(entity.Id, 
                    _localizer[Const.Messages.Saved]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding {@0} {UserId}",
                    command, _userSession.UserId);
            }

            return Result<string>.Fail(_localizer[Const.Messages.InternalError]);
        }
    }

    public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
    {        
        readonly IStringLocalizer _localizer;        

        public CreateContactCommandValidator(            
            IStringLocalizer<CreateContactCommandValidator> localizer)
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

            RuleFor(_ => _.CustomerId)
                .NotEmpty().WithMessage(_localizer["CustomerId is required"]);
            
        }
    }

    public class CreateContactCommandProfile : Profile
    {
        public CreateContactCommandProfile()
        {
            CreateMap<CreateContactCommand, Contact>()
                //.ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                //.ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                //.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                //.ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                //.ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.SearchVector, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .IncludeBase<BaseCreateRequest, AuditableEntity<string>>();
        }
    }
}
