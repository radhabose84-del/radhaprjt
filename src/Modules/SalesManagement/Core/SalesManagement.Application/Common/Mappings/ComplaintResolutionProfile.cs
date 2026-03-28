using AutoMapper;
using SalesManagement.Application.ComplaintResolution.Commands.SubmitResolution;
using SalesManagement.Application.ComplaintResolution.Commands.UpdateResolution;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class ComplaintResolutionProfile : Profile
    {
        public ComplaintResolutionProfile()
        {
            CreateMap<SubmitResolutionCommand, Domain.Entities.ComplaintResolution>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateResolutionCommand, Domain.Entities.ComplaintResolution>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
