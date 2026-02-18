using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class MachineDetailItemsDto : IMapFrom<PreventiveSchedulerItems>
    {
        public string OldItemId { get; set; } = default!;
        public string OldCategoryDescription { get; set; } = default!;
        public string OldGroupName { get; set; } = default!;
        public string? OldItemName { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PreventiveSchedulerItems, MachineDetailItemsDto>();
        }
    }
}
