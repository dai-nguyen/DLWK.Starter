using ApplicationCore;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PointRewardModule.Data;

namespace PointRewardModule.Features.Banks.Commands
{
    public class UpdateBankCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BankType { get; set; } = string.Empty;
    }

    internal class UpdateBankCommandHandler : IRequestHandler<UpdateBankCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly PointRewardModuleDbContext _dbContext;

        public UpdateBankCommandHandler(
           ILogger<UpdateBankCommandHandler> logger,
           IUserSessionService userSession,
           IStringLocalizer<UpdateBankCommandHandler> localizer,
           PointRewardModuleDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            UpdateBankCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _dbContext.Banks.FindAsync(request.Id, cancellationToken);

                if (entity == null)
                {
                    return Result<string>.Fail(_localizer["Bank Not Found"]);
                }

                entity.BankType = request.BankType;

                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(entity.Id, _localizer[Constants.Messages.Saved]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Bank {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer[Constants.Messages.InternalError]);
        }
    }

    public class UpdateBankCommandValidator : AbstractValidator<UpdateBankCommand>
    {
        readonly IStringLocalizer _localizer;

        public UpdateBankCommandValidator(
            IStringLocalizer<UpdateBankCommandValidator> localizer)
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
