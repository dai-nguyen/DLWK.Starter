using ApplicationCore.Entities;
using ApplicationCore.Models;
using AutoMapper;

namespace ApplicationCore.Mappers
{
    public class BaseEntityMapperProfile : Profile
    {
        public BaseEntityMapperProfile() 
        {
            CreateMap<CreateRequestBase, AuditableEntity<string>>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "?"))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => "?"))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId ?? ""));

            CreateMap<UpdateRequestBase, AuditableEntity<string>>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => "?"))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId ?? ""));

            CreateMap<AuditableEntity<string>, ResponseBase>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => src.DateCreated))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.DateUpdated))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId));
        }
    }
}
