using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using Quartz;


namespace Web.Api
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class BulkJobsController : Controller
    {
        readonly ILogger _logger;
        readonly ISchedulerFactory _schedulerFactory;

        public BulkJobsController(
            ILogger<BulkJobsController> logger,
            ISchedulerFactory schedulerFactory)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
        }

        [HttpGet]
        public async Task<Result<GetUserByIdQueryResponse>> Get(string id)
        {
            var query = new GetUserByIdQuery()
            {
                Id = id
            };

            return await _mediator.Send(query);
        }

        //[HttpPost]
        //public async Task<ActionResult<string>> Create(object model)
        //{
        //    IScheduler scheduler = await _schedulerFactory.GetScheduler();

        //    var id = Guid.NewGuid().ToString();

        //    var data = new Dictionary<string, string>();
        //    data.Add("id", id);
        //    data.Add("data", System.Text.Json.JsonSerializer.Serialize(model));
        //    //populate dictionary as per your needs
        //    JobDataMap jobData = new JobDataMap(data);
        //    var jobKey = new JobKey("test_job");
        //    await scheduler.TriggerJob(jobKey, jobData);

        //    return id;
        //}
    }
}
