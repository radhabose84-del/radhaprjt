using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Commands.UpdateFreightRfq
{
    public class UpdateFreightRfqCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int RfqTypeId { get; set; }
        public int? PoReferenceId { get; set; }
        public int? SupplierId { get; set; }
        public string? SourceLocation { get; set; }
        public string? SourceStation { get; set; }
        public string? DestinationLocation { get; set; }
        public string? DestinationStation { get; set; }
        public decimal TotalQuantity { get; set; }
        public int TotalBaleCount { get; set; }
        public int IsActive { get; set; }   // 1=Active, 0=Inactive
    }
}
