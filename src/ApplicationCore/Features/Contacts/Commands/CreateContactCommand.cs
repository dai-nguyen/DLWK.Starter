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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Contacts.Commands
{
    public class CreateContactCommand : CreateRequestBase, IRequest<Result<string>>
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
        readonly IValidator<CreateContactCommand> _validator;

        public CreateContactCommandHandler(
            ILogger<CreateContactCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateContactCommandHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper,
            IValidator<CreateContactCommand> validator)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result<string>> Handle(
            CreateContactCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(command, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Result<string>.Fail(validationResult.Errors.Select(_ => _.ErrorMessage).ToArray());
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
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _appDbContext;
        readonly IMemoryCache _cache;

        public CreateContactCommandValidator(
            ILogger<CreateContactCommandValidator> logger,
            IStringLocalizer<CreateContactCommandValidator> localizer,
            AppDbContext appDbContext,
            IMemoryCache cache)
        {            
            _logger = logger;
            _localizer = localizer;            
            _appDbContext = appDbContext;
            _cache = cache;

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
                .NotEmpty()
                .WithMessage(_localizer["CustomerId is required"])
                .MustAsync((customerId, cancellation) => IsValidCustomerIdAsync(customerId))
                .WithMessage(_localizer["CustomerId must be valid"])
                .When(_ => !string.IsNullOrEmpty(_.CustomerId));            
        }

        private async Task<bool> IsValidCustomerIdAsync(string customerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(customerId))
                    return false;
                
                return await _cache.GetOrCreateAsync(
                    $"IsValidCustomerIdAsync:{customerId}",
                    async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                        return (await _appDbContext.Customers.AnyAsync(_ => _.Id == customerId)) == true;
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for customer id");
            }
            return false;
        }
    }

    public class CreateContactCommandProfile : Profile
    {
        public CreateContactCommandProfile()
        {
            CreateMap<CreateContactCommand, Contact>()                
                .ForMember(dest => dest.SearchVector, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .IncludeBase<CreateRequestBase, AuditableEntity<string>>();
        }
    }
}
