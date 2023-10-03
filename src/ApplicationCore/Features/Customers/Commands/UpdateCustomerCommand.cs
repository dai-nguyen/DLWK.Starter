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
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Customers.Commands
{
    public class UpdateCustomerCommand : UpdateRequestBase, IRequest<Result<string>>
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

    internal class UpdateCustomerCommandHandler :
        IRequestHandler<UpdateCustomerCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;

        public UpdateCustomerCommandHandler(
            ILogger<UpdateCustomerCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateCustomerCommandHandler> localizer,
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
            UpdateCustomerCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Customers.FindAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer[Const.Messages.NotFound]);
                }

                if (entity.Name != command.Name)
                {
                    var isUnique = (await _dbContext.Customers
                        .Where(_ => _.Id != command.Id)
                        .AnyAsync(_ => EF.Functions.ILike(_.Name, command.Name))) == false;

                    if (!isUnique)
                    {
                        return Result<string>.Fail(_localizer["Customer Name is already taken."]);
                    }
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

    public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _appDbContext;
        readonly IMemoryCache _cache;

        public UpdateCustomerCommandValidator(
           ILogger<UpdateCustomerCommandValidator> logger,
           IStringLocalizer<UpdateCustomerCommandValidator> localizer,
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
                .WithMessage(_localizer[$"Name cannot be longer than {CustomerConst.NameMaxLength}"]);

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
    }

    public class UpdateCustomerCommandProfile : Profile
    {
        public UpdateCustomerCommandProfile()
        {
            CreateMap<UpdateCustomerCommand, Customer>()
                .IncludeBase<UpdateRequestBase, AuditableEntity<string>>()
                .ForMember(dest => dest.SearchVector, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore());
        }
    }
}
