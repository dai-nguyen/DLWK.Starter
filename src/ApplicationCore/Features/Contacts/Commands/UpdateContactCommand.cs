using ApplicationCore.Constants.Constants;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
