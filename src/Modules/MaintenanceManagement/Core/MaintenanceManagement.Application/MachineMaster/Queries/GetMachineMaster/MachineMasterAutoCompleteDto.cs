namespace MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster
{
    public class MachineMasterAutoCompleteDto
    {
        public int Id { get; set; }
        public string? MachineName { get; set; }
        public string? MachineCode { get; set; }

        public int MachineGroupId { get; set; }
        public int  DepartmentId { get; set; }
 

    }
}
