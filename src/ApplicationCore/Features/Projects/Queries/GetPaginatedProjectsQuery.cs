using ApplicationCore.Entities;
using ApplicationCore.Features.Contacts.Queries;
using ApplicationCore.Models;
using ApplicationCore.Requests;
using AutoMapper;
using MediatR;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Features.Projects.Queries
{
    public class GetPaginatedProjectsQuery : PaginateRequest,
        IRequest<PaginatedResult<GetPaginatedProjectsQueryResponse>>
    {
        public string CustomerId { get; set; }
        public string ContactId { get; set; }
        public string SearchString { get; set; }

        public string ExternalId { get; set; }

        public GetPaginatedProjectsQuery(
            int pageNumber,
            int pageSize,
            string orderBy,
            string customerId,
            string contactId,
            string searchString,
            string externalId)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            OrderBy = orderBy;
            CustomerId = customerId;
            ContactId = contactId;
            SearchString = searchString;
            ExternalId = externalId;
        }
    }

    internal class GetPaginatedProjectsQueryHandler :
        IRequestHandler<GetPaginatedContactsQuery, PaginatedResult<GetPaginatedProjectsQueryResponse>>
    {
        public Task<PaginatedResult<GetPaginatedProjectsQueryResponse>> Handle(
            GetPaginatedContactsQuery request, 
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    public class GetPaginatedProjectsQueryResponse : BaseResponse
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public Instant DateStart { get; set; }
        public Instant DateDue { get; set; }

        public string CustomerName { get; set; }
    }

    public class GetPaginatedProjectsQueryProfile : Profile
    {
        public GetPaginatedProjectsQueryProfile()
        {
            CreateMap<Project, GetPaginatedProjectsQueryResponse>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
                .IncludeBase<AuditableEntity<string>, BaseResponse>();
        }
    }
}
