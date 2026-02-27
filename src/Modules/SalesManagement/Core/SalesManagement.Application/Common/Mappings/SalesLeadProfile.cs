using AutoMapper;
using SalesManagement.Application.SalesLead.Commands.CreateSalesLead;
using SalesManagement.Application.SalesLead.Commands.UpdateSalesLead;
using SalesManagement.Application.SalesLead.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class SalesLeadProfile : Profile
    {
        public SalesLeadProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.SalesLead, SalesLeadDto>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => src.IsActive  == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            CreateMap<Domain.Entities.SalesLead, SalesLeadLookupDto>();

            // Command to Entity
            CreateMap<CreateSalesLeadCommand, Domain.Entities.SalesLead>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateSalesLeadCommand, Domain.Entities.SalesLead>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
