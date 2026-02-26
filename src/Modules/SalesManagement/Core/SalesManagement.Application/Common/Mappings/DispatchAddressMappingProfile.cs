using AutoMapper;
using SalesManagement.Application.DispatchAddressMapping.Commands.CreateDispatchAddressMapping;
using SalesManagement.Application.DispatchAddressMapping.Commands.UpdateDispatchAddressMapping;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class DispatchAddressMappingProfile : Profile
    {
        public DispatchAddressMappingProfile()
        {
            CreateMap<SalesManagement.Domain.Entities.DispatchAddressMapping,
                      SalesManagement.Application.DispatchAddressMapping.Dto.DispatchAddressMappingDto>();

            CreateMap<SalesManagement.Domain.Entities.DispatchAddressMapping,
                      SalesManagement.Application.DispatchAddressMapping.Dto.DispatchAddressMappingLookupDto>();

            CreateMap<CreateDispatchAddressMappingCommand, SalesManagement.Domain.Entities.DispatchAddressMapping>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateDispatchAddressMappingCommand, SalesManagement.Domain.Entities.DispatchAddressMapping>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
