using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        readonly ILogger _logger;
        readonly ISchedulerFactory _schedulerFactory;

        public UsersController(
            ILogger<UsersController> logger,
            ISchedulerFactory schedulerFactory)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
        }

        // get

        // post

        // put

        // delete

        // bulk
    }
}
