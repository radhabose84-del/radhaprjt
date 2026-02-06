using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById
{
    public class PreventiveSchedulerDtlByIdDto : IMapFrom<PreventiveSchedulerDetail>
    {
        public int PreventiveSchedulerId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PreventiveSchedulerDetail, PreventiveSchedulerDtlByIdDto>()
                .ForMember(dest => dest.PreventiveSchedulerId, opt => opt.MapFrom(src => src.PreventiveSchedulerHeaderId));
        }
    }
}
