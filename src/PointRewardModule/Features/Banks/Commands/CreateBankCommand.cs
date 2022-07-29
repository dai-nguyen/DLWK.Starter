using ApplicationCore.Models;
using FluentValidation;
using MediatR;

namespace PointRewardModule.Features.Banks.Commands
{
    public class CreateBankCommand : IRequest<Result<string>>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();        
        public string BankType { get; set; } = string.Empty;
    }

    public class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
    {

    }
}
