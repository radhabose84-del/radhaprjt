using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.Arrival.Commands.CreateArrival
{
    public class CreateArrivalCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateTimeOffset ArrivalDate { get; set; }
        public int RawMaterialPOId { get; set; }
        public string VehicleNumber { get; set; } = default!;

        // Cross-module FKs
        public int SupplierId { get; set; }
        public int StationId { get; set; }
        public int GodownId { get; set; }
        public int TransporterId { get; set; }

        public decimal? FreightRate { get; set; }
        public string? InvoiceGstNo { get; set; }
        public string? LrNumber { get; set; }
        public string? ContainerNo { get; set; }

        public DateTimeOffset? LorryIn { get; set; }
        public DateTimeOffset? LorryOut { get; set; }

        // Weighbridge — all values supplied in the payload
        public decimal GrossWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal PartyWeight { get; set; }
        public decimal WeightDifference { get; set; }
        public decimal? MoisturePercentage { get; set; }

        public string? Remarks { get; set; }

        public List<CreateArrivalDetailDto> Details { get; set; } = new();
    }

    public class CreateArrivalDetailDto
    {
        public int ItemId { get; set; }
        public int HsnId { get; set; }
        public int PackTypeId { get; set; }
        public int MixCodeId { get; set; }
        public int UomId { get; set; }
        public decimal Rate { get; set; }

        public decimal OrderedQty { get; set; }
        public decimal ArrivedQty { get; set; }
        public decimal CancelledQty { get; set; }
        // BalanceQty is computed in the handler (Ordered − Arrived − Cancelled)

        // Line-level bale summary (persisted on ArrivalDetail). Used as the consolidated
        // range when no per-bale BaleDetails are supplied, and for duplicate-range checks.
        public string BatchNumber { get; set; } = default!;
        public long BaleNumberFrom { get; set; }
        public long BaleNumberTo { get; set; }
        public int TotalBaleCount { get; set; }

        // Per-bale capture array (Individual mode). When EMPTY → Consolidated: the line's
        // [BaleNumberFrom..BaleNumberTo] range is expanded with an even-split weight. When supplied,
        // each entry is saved verbatim as one StockLedgerRaw row. BatchNumber/range live on the line
        // only (not duplicated here).
        public List<ArrivalBaleDto> BaleDetails { get; set; } = new();
    }

    public class ArrivalBaleDto
    {
        public long BaleNumber { get; set; }
        public decimal BaleWeight { get; set; }
        public long? BarcodeNumber { get; set; }        // scanned barcode; null when not scanned
    }
}
