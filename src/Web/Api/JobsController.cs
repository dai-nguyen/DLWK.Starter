using Microsoft.AspNetCore.Mvc;
using Quartz;


namespace Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : Controller
    {
        readonly ILogger _logger;
        readonly ISchedulerFactory _schedulerFactory;

        public JobsController(
            ILogger<JobsController> logger,
            ISchedulerFactory schedulerFactory)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Create(object model)
        {
            IScheduler scheduler = await _schedulerFactory.GetScheduler();

            var id = Guid.NewGuid().ToString();

            var data = new Dictionary<string, string>();
            data.Add("id", id);
            data.Add("data", System.Text.Json.JsonSerializer.Serialize(model));
            //populate dictionary as per your needs
            JobDataMap jobData = new JobDataMap(data);
            var jobKey = new JobKey("test_job");
            await scheduler.TriggerJob(jobKey, jobData);

            return id;
        }
    }
}
