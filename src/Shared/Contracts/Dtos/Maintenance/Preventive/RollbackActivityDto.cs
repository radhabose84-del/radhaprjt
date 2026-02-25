namespace Contracts.Dtos.Maintenance.Preventive
{
    public class RollbackActivityDto
    {
        public int Id { get; set; }
        public int PreventiveSchedulerHeaderId { get; set; }
        public int ActivityId { get; set; }
    }
}