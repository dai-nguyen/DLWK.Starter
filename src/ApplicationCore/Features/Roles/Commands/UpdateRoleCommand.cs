using ApplicationCore.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.Roles.Commands
{
    public class UpdateRoleCommand : IRequest<Result<string>>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    internal class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<string>>
    {


        public Task<Result<string>> Handle(
            UpdateRoleCommand request, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
