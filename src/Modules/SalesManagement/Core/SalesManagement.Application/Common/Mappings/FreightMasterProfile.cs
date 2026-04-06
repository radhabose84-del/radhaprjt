using AutoMapper;
using SalesManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using SalesManagement.Application.FreightMaster.Commands.UpdateFreightMaster;
using SalesManagement.Application.FreightMaster.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class FreightMasterProfile : Profile
    {
        public FreightMasterProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.FreightMaster, FreightMasterDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            CreateMap<Domain.Entities.FreightMaster, FreightMasterLookupDto>();

            // Command to Entity
            CreateMap<CreateFreightMasterCommand, Domain.Entities.FreightMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateFreightMasterCommand, Domain.Entities.FreightMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
