using AutoMapper;
using ProductionManagement.Application.MiscTypeMaster.Commands.CreateMiscTypeMaster;
using ProductionManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using ProductionManagement.Application.MiscTypeMaster.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class MiscTypeMasterProfile : Profile
    {
        public MiscTypeMasterProfile()
        {
            CreateMap<CreateMiscTypeMasterCommand, Domain.Entities.MiscTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscTypeMasterCommand, Domain.Entities.MiscTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.MiscTypeMaster, MiscTypeMasterDto>();
            CreateMap<Domain.Entities.MiscTypeMaster, MiscTypeMasterLookupDto>();
            CreateMap<MiscTypeMasterDto, MiscTypeMasterDto>();
            CreateMap<MiscTypeMasterLookupDto, MiscTypeMasterLookupDto>();
        }
    }
}
