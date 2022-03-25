using ApplicationCore.Features.Users.Commands;
using ApplicationCore.Features.Users.Queries;
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
        
        public UsersController(
            ILogger<UsersController> logger,
            ISchedulerFactory schedulerFactory,
            IMediator mediator)
        {
            _logger = logger;
            _schedulerFactory = schedulerFactory;
            _mediator = mediator;
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


        // bulk
    }
}
