using ApplicationCore;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PointRewardModule.Data;
using PointRewardModule.Entities;

namespace PointRewardModule.Features.Banks.Commands
{
    public class CreateBankCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();        
        public string BankType { get; set; } = string.Empty;
    }

    internal class CreateBankCommandHandler : IRequestHandler<CreateBankCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly PointRewardModuleDbContext _dbContext;

        public CreateBankCommandHandler(
            ILogger<CreateBankCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateBankCommandHandler> localizer,
            PointRewardModuleDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            CreateBankCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = new Bank();
                entity.Id = Guid.NewGuid().ToString();
                entity.BankType = request.BankType;
                entity.Balance = 0;

                _dbContext.Banks.Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(entity.Id, _localizer[Constants.Messages.Saved]);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Bank {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer[Constants.Messages.InternalError]);
        }
    }

    public class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
    {
        readonly IStringLocalizer _localizer;

        public CreateBankCommandValidator(
            IStringLocalizer<CreateBankCommandValidator> localizer)
        {
            _localizer = localizer;

            RuleFor(_ => _.BankType)
                .NotEmpty().WithMessage(_localizer["Bank Type is required"])
                .MaximumLength(100).WithMessage(_localizer["Bank Type cannot be longer than 100 characters"]);            

            RuleFor(_ => _.BankType)
                .NotEmpty().WithMessage(_localizer["Bank Type is required"])
                .MaximumLength(100).WithMessage(_localizer["Bank Type cannot be longer than 100 characters"]);
        }
    }
}
