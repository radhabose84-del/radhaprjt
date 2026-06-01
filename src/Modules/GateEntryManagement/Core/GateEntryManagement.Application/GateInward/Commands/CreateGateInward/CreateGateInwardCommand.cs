using Contracts.Common;
using GateEntryManagement.Application.GateInward.Dto;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Commands.CreateGateInward
{
    public class CreateGateInwardCommand : IRequest<ApiResponseDTO<int>>
    {
        public int? VehicleMovementRecordId { get; set; }
        public int? PartyId { get; set; }
        public int? ReceivingTypeId { get; set; }
        public string? CourierNumber { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? TareWeight { get; set; }

        // Invoice / Delivery Challan (from supplier — all nullable)
        public string? InvoiceNo { get; set; }
        public DateTimeOffset? InvoiceDate { get; set; }
        public string? DcNo { get; set; }
        public DateTimeOffset? DcDate { get; set; }

        // Destination warehouse — required
        public int ReceivingWarehouseId { get; set; }

        public bool QAInspectionRequired { get; set; }
        public int? QAStatusId { get; set; }
        public int UnitId { get; set; }
        public string? Remarks { get; set; }

        public List<CreateGateInwardDetailDto>? GateInwardDetails { get; set; }

        // Single optional Gate Entry Document — staged via upload-attachment first
        public GateInwardAttachmentStageRef? Attachment { get; set; }
    }

    public class GateInwardAttachmentStageRef
    {
        public string FileName { get; set; } = string.Empty;
    }
}
