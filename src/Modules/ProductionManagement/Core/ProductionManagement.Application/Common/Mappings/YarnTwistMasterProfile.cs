using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using ProductionManagement.Application.YarnTwistMaster.Commands.CreateYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.UpdateYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class YarnTwistMasterProfile : Profile
    {
        public YarnTwistMasterProfile()
        {
            CreateMap<CreateYarnTwistMasterCommand, Domain.Entities.YarnTwistMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateYarnTwistMasterCommand, Domain.Entities.YarnTwistMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Domain.Entities.YarnTwistMaster, YarnTwistMasterDto>();
            CreateMap<YarnTwistMasterDto, YarnTwistMasterDto>();
            CreateMap<Domain.Entities.YarnTwistMaster, YarnTwistMasterLookupDto>();
            CreateMap<YarnTwistMasterLookupDto, YarnTwistMasterLookupDto>();
        }
    }
}
