using AutoMapper;
using LogisticsManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using LogisticsManagement.Application.FreightMaster.Commands.UpdateFreightMaster;
using LogisticsManagement.Application.FreightMaster.Dto;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.Application.Common.Mappings
{
    public class FreightMasterProfile : Profile
    {
        public FreightMasterProfile()
        {
            // Entity → DTO (for query handlers that receive entity from repo)
            CreateMap<Domain.Entities.FreightMaster, FreightMasterDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            CreateMap<Domain.Entities.FreightMaster, FreightMasterLookupDto>();

            // CreateCommand → Entity
            CreateMap<CreateFreightMasterCommand, Domain.Entities.FreightMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // UpdateCommand → Entity
            CreateMap<UpdateFreightMasterCommand, Domain.Entities.FreightMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
