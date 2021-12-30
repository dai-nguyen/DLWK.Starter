using ApplicationCore.Data;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Requests;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.Users.Queries
{
    public class GetAllUsersQuery : PaginateRequest, IRequest<PaginatedResult<GetAllUsersQueryResponse>>
    {
        public string SearchString { get; set; }

        public GetAllUsersQuery(
            int pageNumber,
            int pageSize,
            string orderBy,
            string searchString)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            OrderBy = orderBy;
            SearchString = searchString;
        }
    }

    internal class GetAllUsersQueryHandler :
        IRequestHandler<GetAllUsersQuery, PaginatedResult<GetAllUsersQueryResponse>>
    {
        readonly ILogger _logger;
        readonly IUserSession _userSession;
        readonly IStringLocalizer<GetAllUsersQueryHandler> _localizer;
        readonly AppDbContext _dbContext;
        readonly IMapper _mapper;



        public Task<PaginatedResult<GetAllUsersQueryResponse>> Handle(
            GetAllUsersQuery request, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class GetAllUsersQueryResponse
    {

    }
}
