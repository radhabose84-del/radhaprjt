using Contracts.Dtos.Purchase;
using MassTransit;

namespace Contracts.Commands.Sales
{
    public class UpdateApprovedRejectedSalesCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int ModuleTransactionId { get; set; }
        public string ModuleTypeName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public ICollection<UpdateLineStatusDto> LineStatus { get; set; } = default!;
        public int ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
