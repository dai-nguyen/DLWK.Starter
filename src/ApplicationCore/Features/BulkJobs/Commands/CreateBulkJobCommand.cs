using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.BulkJobs.Commands
{
    public partial class CreateBulkJobCommand : IRequest<Result<string>>
    {

    }

    internal class CreateBulkJobCommandHandler : IRequestHandler<CreateBulkJobCommand, Result<string>>
    {
        readonly ILogger _logger;
        readonly IUserSessionService _userSession;
        readonly IStringLocalizer _localizer;



        public Task<Result<string>> Handle(
            CreateBulkJobCommand request, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
