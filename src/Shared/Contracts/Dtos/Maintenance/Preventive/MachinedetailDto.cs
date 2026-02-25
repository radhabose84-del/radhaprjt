namespace Contracts.Dtos.Maintenance.Preventive
{
    public class MachinedetailDto
    {
        public int Id { get; set; }
        // public DateOnly WorkOrderCreationStartDate { get; set; }
        public string HangfireJobId { get; set; } = default!;
    }
}