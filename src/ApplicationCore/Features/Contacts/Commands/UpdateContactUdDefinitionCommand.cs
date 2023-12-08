using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using FluentMigrator.Runner;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Contacts.Commands
{
    public class UpdateContactUdDefinitionCommand : UpdateRequestBase, IRequest<Result<string>>
    {
        public string Label { get; set; }        
        public UserDefinedDataType DataType { get; set; }
        public string[] DropdownValues { get; set; }
    }

    internal class UpdateContactUdDefinitionCommandHandler :
        IRequestHandler<UpdateContactUdDefinitionCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IValidator<UpdateContactUdDefinitionCommand> _validator;
        readonly IMigrationRunner _migrationRunner;

        public UpdateContactUdDefinitionCommandHandler(
            ILogger<UpdateContactUdDefinitionCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateContactUdDefinitionCommandHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper,
            IValidator<UpdateContactUdDefinitionCommand> validator,
            IMigrationRunner migrationRunner)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
            _validator = validator;
            _migrationRunner = migrationRunner;
        }


        public async Task<Result<string>> Handle(
            UpdateContactUdDefinitionCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(command, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Result<string>.Fail(validationResult.Errors.Select(_ => _.ErrorMessage).ToArray());
                }

                var entity = await _dbContext.ContactUdDefinitions.FindAsync(command.Id);

                if (entity == null)                
                    return Result<string>.Fail(_localizer[Const.Messages.NotFound]);                
                
                _mapper.Map(command, entity);
                
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

    public class UpdateContactUdDefinitionCommandValidator : AbstractValidator<UpdateContactUdDefinitionCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;        

        public UpdateContactUdDefinitionCommandValidator(
            ILogger<UpdateContactUdDefinitionCommandValidator> logger, 
            IStringLocalizer<UpdateContactUdDefinitionCommandValidator> localizer)
        {
            _logger = logger;
            _localizer = localizer;            

            RuleFor(_ => _.Label)
                .NotEmpty().WithMessage(localizer["Label is required"])
                .MaximumLength(UserDefinedDefinitionConst.LabelMaxLength)
                .WithMessage(_localizer[$"Label cannot be longer than {UserDefinedDefinitionConst.LabelMaxLength}"]);

            RuleFor(_ => _.DropdownValues)
                .Must((model, dropdownValues) =>
                {
                    if (model.DataType == UserDefinedDataType.Dropdown 
                        && (dropdownValues == null || dropdownValues.Length == 0))
                    {
                        return false;
                    }
                    return true;
                }).WithMessage(_localizer["DropdownValues is required for Dropdown data type"]);
        }
    }

    public class UpdateContactUdDefinitionCommandProfile : Profile
    {
        public UpdateContactUdDefinitionCommandProfile()
        {
            CreateMap<CreateContactUdDefinitionCommand, ContactUdDefinition>()
                .ForMember(dest => dest.DataType, opt => opt.Ignore())
                .IncludeBase<UpdateRequestBase, AuditableEntity<string>>();
        }
    }
}
