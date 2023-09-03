using Microsoft.AspNetCore.Components;
using PointRewardModule.Features.Banks.Queries;

namespace PointRewardModule.Pages.Banks
{
    public partial class Banks
    {
        [Parameter]
        public string UserId { get; set; }

        IEnumerable<GetBankByOwnerIdQueryResponse> _banks = new List<GetBankByOwnerIdQueryResponse>();

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return;
            }

            var request = new GetBanksByOwnerIdQuery()
            {
                OwnerId = UserId
            };

            var res = await _mediator.Send(request);

            _banks = res.Data;
        }
    }
}
