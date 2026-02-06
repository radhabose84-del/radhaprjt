using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById
{
    public class PreventiveSchedulerActivityByIdDto : IMapFrom<PreventiveSchedulerActivity>
    {
        public int Id { get; set; }
        public int PreventiveSchedulerHdrId { get; set; }
        public int ActivityId { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PreventiveSchedulerActivity, PreventiveSchedulerActivityByIdDto>()
                .ForMember(dest => dest.PreventiveSchedulerHdrId, opt => opt.MapFrom(src => src.PreventiveSchedulerHeaderId));
        }
    }
}
