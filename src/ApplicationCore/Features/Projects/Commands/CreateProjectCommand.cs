using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace ApplicationCore.Features.Projects.Commands
{
    public class CreateProjectCommand : CreateRequestBase, IRequest<Result<string>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }
        public Instant DateStart { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public Instant DateDue { get; set; } = SystemClock.Instance.GetCurrentInstant();

        public string CustomerId { get; set; }
        public string ContactId { get; set; }
    }

    internal class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IValidator<CreateProjectCommand> _validator;

        public CreateProjectCommandHandler(
            ILogger<CreateProjectCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateProjectCommandHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper,
            IValidator<CreateProjectCommand> validator)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result<string>> Handle(
            CreateProjectCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(command, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Result<string>.Fail(validationResult.Errors.Select(_ => _.ErrorMessage).ToArray());
                }

                var isValidContact = await _dbContext
                    .Contacts                    
                    .AnyAsync(_ => _.Id == command.ContactId && _.CustomerId == command.CustomerId);

                if (!isValidContact)
                {
                    return Result<string>.Fail(_localizer["Invalid Customer ID and/or Contact ID"]);
                }                

                var entity = _mapper.Map<Project>(command);

                _dbContext.Projects.Add(entity);
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

    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _appDbContext;
        readonly IMemoryCache _cache;

        public CreateProjectCommandValidator(
            ILogger<CreateProjectCommandValidator> logger,
            IStringLocalizer<CreateProjectCommandValidator> localizer,
            AppDbContext appDbContext,
            IMemoryCache cache)
        {
            _logger = logger;
            _localizer = localizer;
            _appDbContext = appDbContext;
            _cache = cache;

            RuleFor(_ => _.Name)
                .NotEmpty().WithMessage(_localizer["Name is required"])
                .MaximumLength(ProjectConst.NameMaxLength)
                .WithMessage(_localizer[$"nAME cannot be longer than {ProjectConst.NameMaxLength}"]);

            RuleFor(_ => _.Description)
                .NotEmpty().WithMessage(_localizer["Description is required"])
                .MaximumLength(ProjectConst.DescriptionMaxLength)
                .WithMessage(_localizer[$"Description cannot be longer than {ProjectConst.DescriptionMaxLength}"]);

            RuleFor(_ => _.Status)
                .NotEmpty().WithMessage(_localizer["Status is required"]);

            RuleFor(_ => _.DateStart)
                .NotEmpty().WithMessage(_localizer["DateStart is required"]);

            RuleFor(_ => _.DateDue)
                .NotEmpty().WithMessage(_localizer["DateDue is required"]);

            RuleFor(_ => _.CustomerId)
                .NotEmpty().WithMessage(_localizer["CustomerId is required"])
                .MustAsync((customerId, cancellation) => IsValidCustomerIdAsync(customerId))
                .WithMessage(_localizer["CustomerId must be valid"])
                .When(_ => !string.IsNullOrEmpty(_.CustomerId));

            RuleFor(_ => _.ContactId)
                .NotEmpty().WithMessage(_localizer["ContactId is required"])
                .MustAsync((contactId, cancellation) => IsValidContactIdAsync(contactId))
                .WithMessage(_localizer["ContactId must be valid"])
                .When(_ => !string.IsNullOrEmpty(_.ContactId));
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

        private async Task<bool> IsValidContactIdAsync(string contactId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(contactId) || string.IsNullOrWhiteSpace(contactId))
                    return false;

                return await _cache.GetOrCreateAsync(
                    $"IsValidContactIdAsync:{contactId}",
                    async entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                        return (await _appDbContext.Contacts.AnyAsync(_ => _.Id == contactId)) == true;
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for contact id");
            }
            return false;
        }
    }

    public class CreateProjectCommandProfile : Profile
    {
        public CreateProjectCommandProfile()
        {
            CreateMap<CreateProjectCommand, Project>()
                .ForMember(dest => dest.SearchVector, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .IncludeBase<CreateRequestBase, AuditableEntity<string>>();
        }
    }
}
