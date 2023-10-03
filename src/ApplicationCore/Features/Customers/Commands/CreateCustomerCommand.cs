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

namespace ApplicationCore.Features.Customers.Commands
{
    public class CreateCustomerCommand : CreateRequestBase, IRequest<Result<string>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Industries { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }

    internal class CreateCustomerCommandHandler :
        IRequestHandler<CreateCustomerCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;

        public CreateCustomerCommandHandler(
            ILogger<CreateCustomerCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateCustomerCommandHandler> localizer,
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
            CreateCustomerCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = _mapper.Map<Customer>(request);

                _dbContext.Customers.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Result<string>.Success(entity.Id,
                    _localizer[Const.Messages.Saved]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating {@0} {UserName}", request, _userSession.UserName);
            }
            return Result<string>.Fail(_localizer[Const.Messages.InternalError]);
        }
    }

    public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _appDbContext;
        readonly IMemoryCache _cache;

        public CreateCustomerCommandValidator(
           ILogger<CreateCustomerCommandValidator> logger,
           IStringLocalizer<CreateCustomerCommandValidator> localizer,
           AppDbContext appDbContext,
           IMemoryCache cache)
        {
            _logger = logger;
            _localizer = localizer;
            _appDbContext = appDbContext;
            _cache = cache;

            RuleFor(_ => _.Name)
                .NotEmpty().WithMessage(_localizer["Name is required"])
                .MaximumLength(CustomerConst.NameMaxLength)
                .WithMessage(_localizer[$"Name cannot be longer than {CustomerConst.NameMaxLength}"])
                .MustAsync((name, cancellation) => IsUniqueNameAsync(name))
                .WithMessage(_localizer["Name must be unique"])
                .When(_ => !string.IsNullOrEmpty(_.Name));

            RuleFor(_ => _.Description)                
                .MaximumLength(CustomerConst.DescriptionMaxLength)
                .WithMessage(_localizer[$"Description cannot be longer than {CustomerConst.DescriptionMaxLength}"]);

            RuleFor(_ => _.Address1)                
                .MaximumLength(CustomerConst.AddressMaxLength)
                .WithMessage(_localizer[$"Address 1 cannot be longer than {CustomerConst.AddressMaxLength}"]);

            RuleFor(_ => _.Address2)
                .MaximumLength(CustomerConst.AddressMaxLength)
                .WithMessage(_localizer[$"Address 2 cannot be longer than {CustomerConst.AddressMaxLength}"]);

            RuleFor(_ => _.City)
                .MaximumLength(CustomerConst.CityMaxLength)
                .WithMessage(_localizer[$"City cannot be longer than {CustomerConst.CityMaxLength}"]);

            RuleFor(_ => _.State)
                .MaximumLength(CustomerConst.StateMaxLength)
                .WithMessage(_localizer[$"State cannot be longer than {CustomerConst.StateMaxLength}"]);

            RuleFor(_ => _.Country)
                .MaximumLength(CustomerConst.CountryMaxLength)
                .WithMessage(_localizer[$"Country cannot be longer than {CustomerConst.CountryMaxLength}"]);

        }

        private async Task<bool> IsUniqueNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(name))
                    return false;

                name = name.Trim().ToLower();

                return await _cache.GetOrCreateAsync(
                    $"IsUniqueCustomerName:{name.Trim().ToLower()}",
                    async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                        return (await _appDbContext.Customers.AnyAsync(_ => EF.Functions.ILike(_.Name, name))) == false;
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for unique customer name");
            }
            return false;
        }
    }

    public class CreateCustomerCommandProfile : Profile
    {
        public CreateCustomerCommandProfile() 
        {
            CreateMap<CreateCustomerCommand, Customer>()
                .ForMember(_ => _.SearchVector, opt => opt.Ignore())
                .IncludeBase<CreateRequestBase, AuditableEntity<string>>();
        }

    }
}
