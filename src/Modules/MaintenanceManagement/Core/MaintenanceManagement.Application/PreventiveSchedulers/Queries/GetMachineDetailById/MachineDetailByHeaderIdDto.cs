namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class MachineDetailByHeaderIdDto
    {
        public int HeaderId { get; set; }
        public int DetailId { get; set; }
        public int WorkOrderId { get; set; }
        public string PreventiveSchedulerName { get; set; } = default!;
        public int MachineGroupId { get; set; }
        public string GroupName { get; set; } = default!;
        public int MachineId { get; set; }
        public string MachineName { get; set; } = default!;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = default!;
        public string MachineCode { get; set; } = default!;
    }
}