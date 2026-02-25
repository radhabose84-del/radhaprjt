namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetUnMappedMachine
{
    public class UnMappedMachineDto
    {
        public int Id { get; set; }
        public string MachineCode { get; set; } = default!;
        public string MachineName { get; set; } = default!;
    }
}