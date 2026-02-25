namespace MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster
{
    public class CreateActivityMasterDto
    {
       
        public string? ActivityName { get; set;}
        public string? Description { get; set; }
        public int  UnitId  {get; set; }
        public int DepartmentId { get; set; }
        public int EstimatedDuration { get; set; }
        public int ActivityType { get; set; }
    

        public List<ActivityMachineGroupDto>? ActivityMachineGroup { get; set; }


    }
}