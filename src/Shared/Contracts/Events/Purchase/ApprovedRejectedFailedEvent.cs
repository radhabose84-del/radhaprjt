using Contracts.Dtos.Purchase;

namespace Contracts.Events.Purchase
{
    public class ApprovedRejectedFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public int IndentId { get; set; }
        public string Reason { get; set; } = default!;
        public ICollection<UpdateLineStatusDto> LineStatus { get; set; } = default!;

    }
}