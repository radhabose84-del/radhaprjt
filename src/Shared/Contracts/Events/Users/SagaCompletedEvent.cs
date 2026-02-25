namespace Contracts.Events.Users
{
    public class SagaCompletedEvent
    {

        public Guid CorrelationId { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }
    }
}