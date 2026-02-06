using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class MachineDetailItemsDto : IMapFrom<PreventiveSchedulerItems>
    {
        public string OldItemId { get; set; }
        public string OldCategoryDescription { get; set; }
        public string OldGroupName { get; set; }
        public string? OldItemName { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PreventiveSchedulerItems, MachineDetailItemsDto>();
        }
    }
}
