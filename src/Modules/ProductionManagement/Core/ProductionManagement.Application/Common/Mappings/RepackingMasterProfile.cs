using AutoMapper;
using ProductionManagement.Application.RepackingMaster.Commands.CreateRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Commands.UpdateRepackingMaster;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class RepackingMasterProfile : Profile
    {
        public RepackingMasterProfile()
        {
            CreateMap<CreateRepackingMasterCommand, Domain.Entities.RepackingMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateRepackingMasterCommand, Domain.Entities.RepackingMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
