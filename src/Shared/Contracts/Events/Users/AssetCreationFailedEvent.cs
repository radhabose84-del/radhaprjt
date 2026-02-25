namespace Contracts.Events.Users
{
    public class AssetCreationFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; } = default!;
    }
}