using AutoMapper;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class RepackingHeaderProfile : Profile
    {
        public RepackingHeaderProfile()
        {
            CreateMap<CreateRepackingDetailItem, Domain.Entities.RepackingDetail>();

            CreateMap<CreateRepackingHeaderCommand, Domain.Entities.RepackingHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.RepackingDetails, opt => opt.MapFrom(src => src.Details));

            CreateMap<UpdateRepackingHeaderCommand, Domain.Entities.RepackingHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.RepackingDetails, opt => opt.MapFrom(src => src.Details));
        }
    }
}
