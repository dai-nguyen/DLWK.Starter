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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Contacts.Commands
{
    public class CreateContactUdDefinitionCommand : CreateRequestBase, IRequest<Result<string>>
    {
        public string Label { get; set; }
        public string Code { get; set; }
        public UserDefinedDataType DataType { get; set; }
        public string[] DropdownValues { get; set; }
    }

    internal class CreateContactUdDefinitionCommandHandler :
        IRequestHandler<CreateContactUdDefinitionCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;
        readonly IValidator<CreateContactUdDefinitionCommand> _validator;
        readonly IMigrationRunner _migrationRunner;

        public CreateContactUdDefinitionCommandHandler(
            ILogger<CreateContactUdDefinitionCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateContactUdDefinitionCommandHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper,
            IValidator<CreateContactUdDefinitionCommand> validator,
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
            CreateContactUdDefinitionCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(command, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return Result<string>.Fail(validationResult.Errors.Select(_ => _.ErrorMessage).ToArray());
                }

                var entity = _mapper.Map<ContactUdDefinition>(command);

                _dbContext.ContactUdDefinitions.Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                _migrationRunner.MigrateUp();
                
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

    public class CreateContactUdDefinitionCommandValidator : AbstractValidator<CreateContactUdDefinitionCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _appDbContext;
        readonly IMemoryCache _cache;

        public CreateContactUdDefinitionCommandValidator(
            ILogger<CreateContactUdDefinitionCommandValidator> logger, 
            IStringLocalizer localizer, 
            AppDbContext appDbContext, 
            IMemoryCache cache)
        {
            _logger = logger;
            _localizer = localizer;
            _appDbContext = appDbContext;
            _cache = cache;

            RuleFor(_ => _.Label)
                .NotEmpty().WithMessage(localizer["Label is required"])
                .MaximumLength(UserDefinedDefinitionConst.LabelMaxLength)
                .WithMessage(_localizer[$"Label cannot be longer than {UserDefinedDefinitionConst.LabelMaxLength}"]);

            RuleFor(_ => _.Code)
                .NotEmpty().WithMessage(localizer["Code is required"])
                .MaximumLength(UserDefinedDefinitionConst.CodeMaxLength)
                .WithMessage(_localizer[$"Label cannot be longer than {UserDefinedDefinitionConst.CodeMaxLength}"])
                .Must(IsUniqueCode)
                .WithMessage(_localizer["Contact UD Code must be unique"])
                .When(_ => !string.IsNullOrEmpty(_.Code));
        }

        private bool IsUniqueCode(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(code))
                    return false;

                code = code.Trim().ToLower();

                return _cache.GetOrCreate(
                    $"ContactUd:IsUniqueCode:{code.Trim().ToLower()}",
                    entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                        return _appDbContext.ContactUdDefinitions.Any(_ => EF.Functions.ILike(_.Code, code)) == false;
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for unique Contact UD Code");
            }
            return false;
        }
    }

    public class CreateContactUdDefinitionCommandProfile : Profile
    {
        public CreateContactUdDefinitionCommandProfile()
        {
            CreateMap<CreateContactUdDefinitionCommand, ContactUdDefinition>()
                .IncludeBase<CreateRequestBase, AuditableEntity<string>>();
        }
    }
}
