using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.OCREntry.Commands.UpdateOCREntry
{
    public class UpdateOCREntryCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public DateTimeOffset OcrDate { get; set; }

        // Same-module
        public int ProcurementSourceId { get; set; }
        public int ProcurementTypeId { get; set; }
        public int BrokerDirectId { get; set; }
        public string? BrokerName { get; set; }
        public int? GradeId { get; set; }
        public int PaymentTermId { get; set; }

        // Cross-module
        public int SupplierId { get; set; }
        public int LocationId { get; set; }
        public int StationId { get; set; }
        public int ItemId { get; set; }
        public int CountId { get; set; }

        // Cotton commercial details
        public decimal Quantity { get; set; }
        public decimal? Weight { get; set; }
        public decimal Rate { get; set; }
        public DateTimeOffset? ExpectedDispatchDate { get; set; }
        public string? DocumentPath { get; set; }

        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive
    }
}
