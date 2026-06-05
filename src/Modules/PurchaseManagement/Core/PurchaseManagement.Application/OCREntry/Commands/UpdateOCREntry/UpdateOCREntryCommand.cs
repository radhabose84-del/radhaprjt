using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.OCREntry.Dto;

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

        // Additional Cotton Details — same-module MiscMaster FKs (optional)
        public int? PaymentModeId { get; set; }
        public int? WeighmentId { get; set; }
        public int? TransitInsuranceId { get; set; }
        public int? LorryFreightId { get; set; }

        // Rate unit — cross-module Inventory UOM (optional)
        public int? UomId { get; set; }

        // Additional Cotton Details — scalar fields
        public string? MillSampleNo { get; set; }
        public string? CottonPassedBy { get; set; }
        public decimal? GstPercentage { get; set; }
        public string? Remarks { get; set; }

        // Quality template + dynamic cotton-quality parameter values (replace-all on update)
        public int? QualityTemplateId { get; set; }
        public List<OCRQualityParameterInputDto>? QualityParameters { get; set; }

        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive
    }
}
