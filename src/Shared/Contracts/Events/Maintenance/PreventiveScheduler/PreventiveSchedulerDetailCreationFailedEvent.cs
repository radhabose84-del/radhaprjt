namespace Contracts.Events.Maintenance.PreventiveScheduler
{
    public class PreventiveSchedulerDetailCreationFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; } = default!;
        public string token { get; set; } = default!;
    }
}