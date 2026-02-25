namespace Contracts.Events.Users
{
    public class UserCreationFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; } = default!;
    }
}