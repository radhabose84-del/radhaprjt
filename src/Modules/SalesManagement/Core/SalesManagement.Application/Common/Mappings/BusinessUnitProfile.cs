
using AutoMapper;
using SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
using SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit;
using SalesManagement.Application.BusinessUnit.Dto;

namespace SalesManagement.Application.Common.Mappings
{
    public class BusinessUnitProfile : Profile
    {
        public BusinessUnitProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.BusinessUnit, BusinessUnitDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Domain.Common.BaseEntity.Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == Domain.Common.BaseEntity.IsDelete.Deleted));

            CreateMap<Domain.Entities.BusinessUnit, BusinessUnitLookupDto>();

            // Command to Entity
            CreateMap<CreateBusinessUnitCommand, Domain.Entities.BusinessUnit>();
            CreateMap<UpdateBusinessUnitCommand, Domain.Entities.BusinessUnit>();
        }
    }
}
