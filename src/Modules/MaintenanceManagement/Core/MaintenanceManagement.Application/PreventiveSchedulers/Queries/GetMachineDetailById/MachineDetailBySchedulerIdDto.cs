using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class MachineDetailBySchedulerIdDto : IMapFrom<PreventiveSchedulerDetail>
    {
        public int Id { get; set; }
        public string MachineCode { get; set; } = default!;
        public string MachineName { get; set; } = default!;
        public DateOnly WorkOrderCreationStartDate { get; set; }
        public DateOnly LastMaintenanceActivityDate { get; set; }
        public DateOnly ActualWorkOrderDate { get; set; }
        public int FrequencyInterval { get; set; }
        public byte IsActive { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PreventiveSchedulerDetail, MachineDetailBySchedulerIdDto>()
                .ForMember(dest => dest.MachineCode, opt => opt.MapFrom(src => src.Machine.MachineCode))
                .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine.MachineName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (byte)src.IsActive));
        }
    }
}
