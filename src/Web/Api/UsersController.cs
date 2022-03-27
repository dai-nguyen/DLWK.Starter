using ApplicationCore;
using ApplicationCore.Features.BulkJobs.Commands;
using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Features.Users.Queries;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using Quartz;

namespace Web.Api
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        readonly ILogger _logger;
        readonly ISchedulerFactory _schedulerFactory;
        readonly IMediator _mediator;
        readonly IUserSessionService _userSession;
        
        public UsersController(
            ILogger<UsersController> logger,
            ISchedulerFactory schedulerFactory,
            IMediator mediator,
            IUserSessionService userSession)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
            _mediator = mediator;
            _userSession = userSession;
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

        [HttpPost]
        public async Task<Result<string>> Post(CreateUserCommand command)
        {
            return await _mediator.Send(command);
        }
        
        [HttpPut]
        public async Task<Result<string>> Put(UpdateUserCommand command)
        {
            return await _mediator.Send(command);
        }
        
        [HttpDelete]
        public async Task<Result<string>> Delete(DeleteUserCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost]
        public async Task<Result<string>> Bulk(BulkUserCommand command)
        {
            try
            {
                var jobCommand = new CreateBulkJobCommand();
                var jobRes = await _mediator.Send(jobCommand);

                if (!jobRes.Succeeded)
                {
                    return jobRes;
                }

                IScheduler scheduler = await _schedulerFactory.GetScheduler();

                var id = jobRes.Data;

                var data = new Dictionary<string, string>();
                data.Add("id", id);
                data.Add("data", System.Text.Json.JsonSerializer.Serialize(command));
                JobDataMap jobData = new JobDataMap(data);
                var jobKey = new JobKey(Constants.ClaimNames.users);
                await scheduler.TriggerJob(jobKey, jobData);

                return Result<string>.Success(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk user {@0) {UserId}",
                    command, _userSession.UserId);
            }
            return Result<string>.Fail("Internal Server Error");
        }
    }
}
