using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace ApplicationCore.Features.Projects.Commands
{
    public class UpdateProjectCommand : UpdateRequestBase, IRequest<Result<string>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectStatus Status { get; set; }
        public Instant DateStart { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public Instant DateDue { get; set; } = SystemClock.Instance.GetCurrentInstant();
    }

    internal class UpdateProjectCommandHandler :
        IRequestHandler<UpdateProjectCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IValidator<UpdateProjectCommand> _validator;

        public UpdateProjectCommandHandler(
            ILogger<UpdateProjectCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateProjectCommandHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper,
            IValidator<UpdateProjectCommand> validator)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result<string>> Handle(
            UpdateProjectCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(command, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Result<string>.Fail(validationResult.Errors.Select(_ => _.ErrorMessage).ToArray());
                }

                var entity = await _dbContext.Projects.FindAsync(command.Id);

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

    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        readonly IStringLocalizer _localizer;

        public UpdateProjectCommandValidator(
            IStringLocalizer<UpdateProjectCommandValidator> localizer)
        {
            _localizer = localizer;

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
            
        }
    }

    public class UpdateProjectCommandProfile : Profile
    {
        public UpdateProjectCommandProfile()
        {
            CreateMap<UpdateProjectCommand, Project>()
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ContactId, opt => opt.Ignore())
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.SearchVector, opt => opt.Ignore())
                .IncludeBase<UpdateRequestBase, AuditableEntity<string>>();
        }
    }
}
