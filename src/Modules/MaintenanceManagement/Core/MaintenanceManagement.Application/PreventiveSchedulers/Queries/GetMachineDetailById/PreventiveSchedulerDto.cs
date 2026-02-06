using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Domain.Entities;
using System.Collections.Generic;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class PreventiveSchedulerDto : IMapFrom<PreventiveSchedulerHeader>
    {
        public int Id { get; set; }
        public string PreventiveSchedulerName { get; set; }
        public string GroupName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int FrequencyInterval { get; set; }
        public int GraceDays { get; set; }
        public int ReminderWorkOrderDays { get; set; }
        public int ReminderMaterialReqDays { get; set; }
        public int IsDownTimeRequired { get; set; }
        public decimal DownTimeEstimateHrs { get; set; }

        public required List<MachineDetailActivityDto> Activity { get; set; }
        public List<MachineDetailItemsDto>? Items { get; set; }
        public required List<MachineDetailBySchedulerIdDto> PreventiveSchedulerDtl { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PreventiveSchedulerHeader, PreventiveSchedulerDto>()
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.MachineGroup.GroupName))
                .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => src.PreventiveSchedulerActivities))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PreventiveSchedulerItems))
                .ForMember(dest => dest.PreventiveSchedulerDtl, opt => opt.MapFrom(src => src.PreventiveSchedulerDetails))
                .ForMember(dest => dest.IsDownTimeRequired, opt => opt.MapFrom(src => (int)src.IsDownTimeRequired));
        }
    }
}
