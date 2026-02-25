using Contracts.Dtos.Purchase;
using MassTransit;

namespace Contracts.Commands.Purchase
{
    public class UpdateIndentDetailCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int IndentId { get; set; }
        public string Status { get; set; } = default!;
        public ICollection<UpdateLineStatusDto> LineStatus { get; set; } = default!;
    }
}