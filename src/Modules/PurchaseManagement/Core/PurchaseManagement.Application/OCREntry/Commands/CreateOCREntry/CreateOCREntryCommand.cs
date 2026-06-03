using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry
{
    public class CreateOCREntryCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateTimeOffset OcrDate { get; set; }

        // Same-module (MiscMaster / PaymentTermMaster)
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
    }
}
