namespace Contracts.Events.Users
{
    public class UserCreatedEvent
    {
        public Guid CorrelationId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}