using ApplicationCore.Constants;
using ApplicationCore.Data;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ApplicationCore.Features.Contacts.Commands
{
    public class CreateContactUdDefinitionCommand : IRequest<Result<string>>
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
        

        public CreateContactUdDefinitionCommandHandler(
            ILogger<CreateContactUdDefinitionCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateContactUdDefinitionCommandHandler> localizer,
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
            CreateContactUdDefinitionCommand command, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
                .WithMessage(_localizer[$"Label cannot be longer than {UserDefinedDefinitionConst.CodeMaxLength}"]);
        }
    }
}
