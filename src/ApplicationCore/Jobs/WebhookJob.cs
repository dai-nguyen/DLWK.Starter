using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ApplicationCore.Jobs
{
    public class WebhookJob : IJob
    {
        readonly ILogger _logger;
        readonly IMediator _mediator;

        public WebhookJob(
            ILogger<WebhookJob> logger, 
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Webhook job executing");

            // get list of webhook items with less than x failed count and webhook setup is enabled
            // each failed count will try in increment of 5 minutes.
            // ex: if failed count is 1 then next try should be 5 minutes after the 1st try
            // send post request
            // if not 200 response then increment failed count
            // if failed count exceed x then disable Webhook setup

            await Task.FromResult(0);
        }
    }
}
