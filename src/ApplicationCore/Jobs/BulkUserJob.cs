using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Features.BulkJobs.Commands;

namespace ApplicationCore.Jobs
{
    public class BulkUserJob : IJob
    {
        readonly ILogger _logger;
        readonly IMediator _mediator;

        public BulkUserJob(
            ILogger<BulkUserJob> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // get job id from the context
            JobKey key = context.JobDetail.Key;

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var id = dataMap.GetString("id");
            var data = dataMap.GetString("data"); 
            var command = JsonSerializer.Deserialize<BulkUserCommand>(data);
            var res = await _mediator.Send(command);

            var updateBulkJobCommand = new UpdateBulkJobCommand()
            {
                Id = id,
                Status = Constants.BulkJobStatus.Completed,
                Error = 
            }

            var job = _mediator.Send(new UpdateUserCommand)

        }
    }
}
