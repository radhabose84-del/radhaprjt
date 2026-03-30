using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.Power;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;



namespace MaintenanceManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {

        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public new Status IsActive { get; set; } = Status.Active;
        public MiscTypeMaster? MiscTypeMaster { get; set; }
        public ICollection<MaintenanceRequest>? RequestType { get; set; }
        public ICollection<MaintenanceRequest>? MaintenanceType { get; set; }
        public ICollection<WorkOrder>? WorkOrderStatus { get; set; }
        public ICollection<WorkOrder>? WorkOrderRootCause { get; set; }
        public ICollection<WorkOrderItem>? WorkOrderItemStoreType { get; set; }
        public ICollection<WorkOrderTechnician>? WorkOrderTechnicianSource { get; set; }
        public ICollection<PreventiveSchedulerHeader>? Schedule { get; set; }
        public ICollection<PreventiveSchedulerHeader>? FrequencyType { get; set; }
        public ICollection<PreventiveSchedulerHeader>? FrequencyUnit { get; set; }
        public ICollection<MaintenanceRequest>? ServiceType { get; set; }
        public ICollection<MaintenanceRequest>? ServiceLocation { get; set; }
        public ICollection<MaintenanceRequest>? SpareType { get; set; }

        public ICollection<MaintenanceRequest>? RequestStatus { get; set; }

        public ICollection<MaintenanceRequest>? ModeOfDispatchType { get; set; }
        public ICollection<PreventiveSchedulerHeader>? MaintenanceCategory { get; set; }
        public ICollection<WorkOrderSchedule>? WorkOrderScheduleStatus { get; set; }

        public ICollection<ActivityMaster>? ActivityType { get; set; }
        public ICollection<MachineMaster>? MachineMasterLineNo { get; set; }

        public ICollection<Feeder>? Feeders { get; set; }
        public ICollection<PowerConsumption>? FeedersPower { get; set; }

        public ICollection<PreventiveSchedulerDetail>? PreventiveDetailSchedule { get; set; }
        public ICollection<PreventiveSchedulerDetail>? PreventiveDetailFrequencyType { get; set; }
        public ICollection<PreventiveSchedulerDetail>? PreventiveDetailFrequencyUnit { get; set; }
        public ICollection<MachineSpecification>? MachineSpecificationsName { get; set; }

      
        public ICollection<GeneratorConsumption>? GeneratorConsumptions { get; set; }




        




		
  		    
    }
}