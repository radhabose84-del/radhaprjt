
using AutoMapper;
using SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
using SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit;
using SalesManagement.Application.BusinessUnit.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class BusinessUnitProfile : Profile
    {
        public BusinessUnitProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.BusinessUnit, BusinessUnitDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            CreateMap<Domain.Entities.BusinessUnit, BusinessUnitLookupDto>();

            // Command to Entity
            CreateMap<CreateBusinessUnitCommand, Domain.Entities.BusinessUnit>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateBusinessUnitCommand, Domain.Entities.BusinessUnit>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
