using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class MachineDetailActivityDto : IMapFrom<PreventiveSchedulerActivity>
    {
        public string ActivityName { get; set; } = default!;

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PreventiveSchedulerActivity, MachineDetailActivityDto>()
                .ForMember(dest => dest.ActivityName, opt => opt.MapFrom(src => src.Activity.ActivityName));
        }
    }
}
