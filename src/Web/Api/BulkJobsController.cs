using ApplicationCore.Features.BulkJobs.Queries;
using ApplicationCore.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;


namespace Web.Api
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class BulkJobsController : Controller
    {
        readonly ILogger _logger;
        readonly IMediator _mediator;

        public BulkJobsController(
            ILogger<BulkJobsController> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<Result<GetBulkJobByIdQueryResponse>> Get(string id)
        {
            var query = new GetBulkJobByIdQuery()
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
