using Contracts.Dtos.Purchase;
using MassTransit;

namespace Contracts.Commands.Workflow
{
    public class RollbackApprovedRejectStatus : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int IndentId { get; set; }
        public string Reason { get; set; } = default!;
        public ICollection<UpdateLineStatusDto> RollbackApprovedRejected { get; set; } = default!;
        
    }
}