using ApplicationCore;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PointRewardModule.Data;
using PointRewardModule.Entities;

namespace PointRewardModule.Features.Transactions.Commands
{
    public class CreateTransactionCommand : IRequest<Result<string>>
    {        
        public string BankId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    internal class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;
        readonly PointRewardModuleDbContext _dbContext;

        public CreateTransactionCommandHandler(
            ILogger<CreateTransactionCommandHandler> logger,
            IUserSessionService userSession,
            IStringLocalizer<CreateTransactionCommandHandler> localizer,
            PointRewardModuleDbContext dbContext)
        {
            _logger = logger;
            _userSession = userSession;
            _localizer = localizer;
            _dbContext = dbContext;
        }

        public async Task<Result<string>> Handle(
            CreateTransactionCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                var entity = new Transaction();
                entity.Id = Guid.NewGuid().ToString();
                entity.BankId = request.BankId;
                entity.Amount = request.Amount;
                entity.Notes = request.Notes;

                _dbContext.Transactions.Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(entity.Id, _localizer[Constants.Messages.Saved]);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Transaction {@0) {UserId}",
                    request, _userSession.UserId);
            }
            return Result<string>.Fail(_localizer[Constants.Messages.InternalError]);
        }
    }

    public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        readonly ILogger _logger;
        readonly IStringLocalizer _localizer;
        readonly PointRewardModuleDbContext _dbContext;
        readonly IMemoryCache _cache;

        public CreateTransactionCommandValidator(
            ILogger<CreateTransactionCommandValidator> logger,
            IStringLocalizer<CreateTransactionCommandValidator> localizer,
            PointRewardModuleDbContext dbContext,
            IMemoryCache cache)
        {
            _logger = logger;
            _localizer = localizer;
            _dbContext = dbContext;
            _cache = cache;


            RuleFor(_ => _.BankId)
                .NotEmpty().WithMessage(_localizer["Bank Id is required"])
                .Must((bankId) => IsValidBankId(bankId))
                .When(_ => !string.IsNullOrEmpty(_.BankId));

            RuleFor(_ => _.Amount)
                .Must((amount) => amount != 0)
                .WithMessage(_localizer["Amount must be different than 0"]);

            RuleFor(_ => _.Notes)                
                .MaximumLength(255)
                .When(_ => !string.IsNullOrEmpty(_.Notes))
                .WithMessage(_localizer["Notes must not exceeds 255 characters"]);
        }

        private bool IsValidBankId(string bankId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bankId) || string.IsNullOrWhiteSpace(bankId))
                    return false;
                
                return _cache.GetOrCreate(
                    $"IsValidBankId:{bankId}",
                    entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                        return _dbContext.Banks.Any(_ => _.Id == bankId);
                    });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for valid bank id");
            }
            return false;
        }
    }
}
