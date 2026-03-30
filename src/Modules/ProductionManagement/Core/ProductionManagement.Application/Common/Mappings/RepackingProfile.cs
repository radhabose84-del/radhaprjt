using AutoMapper;
using ProductionManagement.Application.Repacking.Commands.CreateRepacking;
using ProductionManagement.Application.Repacking.Commands.UpdateRepacking;
using ProductionManagement.Domain.Entities;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class RepackingProfile : Profile
    {
        public RepackingProfile()
        {
            CreateMap<CreateRepackingCommand, RepackingHeader>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(_ => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => IsDelete.NotDeleted))
                .ForMember(dest => dest.RepackingDetails, opt => opt.MapFrom(src => src.RepackingDetails));

            CreateMap<CreateRepackingDetailDto, RepackingDetail>();

            CreateMap<UpdateRepackingCommand, RepackingHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.RepackingDetails, opt => opt.MapFrom(src => src.RepackingDetails));
        }
    }
}
