using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq
{
    public class CreateFreightRfqCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateTimeOffset RfqDate { get; set; }
        public int RfqTypeId { get; set; }
        public int? PoReferenceId { get; set; }      // required when type = PO Based (validator)
        public int? SupplierId { get; set; }         // snapshot from PO
        public string? SourceLocation { get; set; }
        public string? SourceStation { get; set; }
        public string? DestinationLocation { get; set; }
        public string? DestinationStation { get; set; }
        public decimal TotalQuantity { get; set; }
        public int TotalBaleCount { get; set; }
    }
}
