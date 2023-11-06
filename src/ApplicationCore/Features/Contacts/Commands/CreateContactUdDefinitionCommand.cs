using ApplicationCore.Data;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        readonly IValidator<CreateContactUdDefinitionCommand> _validator;

        public CreateContactUdDefinitionCommandHandler(
            ILogger<CreateContactUdDefinitionCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateContactUdDefinitionCommandHandler> localizer,
            AppDbContext dbContext,
            IMapper mapper,
            IValidator<CreateContactUdDefinitionCommand> validator)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
            _mapper = mapper;
            _validator = validator;
        }


        public async Task<Result<string>> Handle(
            CreateContactUdDefinitionCommand command, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
