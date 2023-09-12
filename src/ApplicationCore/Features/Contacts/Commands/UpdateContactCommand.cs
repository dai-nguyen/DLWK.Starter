using ApplicationCore.Constants;
using ApplicationCore.Constants.Constants;
using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ApplicationCore.Features.Contacts.Commands
{
    public class UpdateContactCommand : IRequest<Result<string>>
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    internal class UpdateContactCommandHandler :
        IRequestHandler<UpdateContactCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly AppDbContext _dbContext;

        public UpdateContactCommandHandler(
            ILogger<UpdateContactCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<UpdateContactCommandHandler> localizer,
            AppDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            UpdateContactCommand command, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Contacts.FindAsync(command.Id);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer[Const.Messages.NotFound]);
                }

                entity.FirstName = command.FirstName;
                entity.LastName = command.LastName;
                entity.Email = command.Email;
                entity.Phone = command.Phone;

                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(entity.Id,
                    _localizer[Const.Messages.Saved]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Contact {@0} {UserId}",
                    command, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer[Const.Messages.InternalError]);
        }
    }

    public class UpdateContactCommandValidator : AbstractValidator<UpdateContactCommand>
    {
        readonly IStringLocalizer _localizer;

        public UpdateContactCommandValidator(
            IStringLocalizer<UpdateContactCommandValidator> localizer)
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

            RuleFor(_ => _.Id)
                .NotEmpty().WithMessage(_localizer["Contact is required"]);
        }
    }
}
