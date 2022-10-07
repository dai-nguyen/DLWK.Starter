using ApplicationCore.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointRewardModule.Features.Transactions.Queries
{
    public class GetTransactionByIdQuery : IRequest<Result<GetTransactionByIdQueryResponse>>
    {
        public string Id { get; set; } = string.Empty;
    }

    internal class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, Result<GetTransactionByIdQueryResponse>>
    {
        public Task<Result<GetTransactionByIdQueryResponse>> Handle(
            GetTransactionByIdQuery request, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class GetTransactionByIdQueryResponse
    {
        public string Id { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public int Amount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
