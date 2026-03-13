using AutoMapper;
using ProductionManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using ProductionManagement.Application.MiscMaster.Commands.UpdateMiscMaster;
using ProductionManagement.Application.MiscMaster.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class MiscMasterProfile : Profile
    {
        public MiscMasterProfile()
        {
            CreateMap<CreateMiscMasterCommand, Domain.Entities.MiscMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, Domain.Entities.MiscMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.MiscMaster, MiscMasterDto>();
            CreateMap<Domain.Entities.MiscMaster, MiscMasterLookupDto>();
            CreateMap<MiscMasterDto, MiscMasterDto>();
            CreateMap<MiscMasterLookupDto, MiscMasterLookupDto>();
        }
    }
}
