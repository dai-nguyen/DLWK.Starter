using ApplicationCore.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.Customers.Commands
{
    public class CreateCustomerUdDefinitionCommand : CreateRequestBase, IRequest<Result<string>>
    {
    }
}
