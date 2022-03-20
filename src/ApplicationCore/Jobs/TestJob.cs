using Microsoft.Extensions.Logging;
using Quartz;

namespace ApplicationCore.Jobs
{
    public class TestJob : IJob
    {
        readonly ILogger _logger;

        public TestJob(
            ILogger<TestJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // get job id from the context
            JobKey key = context.JobDetail.Key;

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            //var data = dataMap.GetString("data");

            // simulate
            await Task.Delay(3000);                 
        }
    }
}
