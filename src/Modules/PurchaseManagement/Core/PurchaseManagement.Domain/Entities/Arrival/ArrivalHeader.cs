using System.ComponentModel.DataAnnotations.Schema;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.RawMaterialPO;

namespace PurchaseManagement.Domain.Entities.Arrival
{
    /// <summary>
    /// Cotton bale inward (Arrival) transaction against a Raw Material PO / Agreement.
    /// Table: [Purchase].[ArrivalHeader].
    /// </summary>
    public class ArrivalHeader : BaseEntity
    {
        public int UnitId { get; set; }

        // System generated, unique, immutable (DocumentSequence / TransactionType master)
        public string ArrivalNumber { get; set; } = default!;
        public DateTimeOffset ArrivalDate { get; set; }

        // ── Same-module FK (RawMaterialPOHeader) — DB constraint, Dapper JOIN on read ──
        public int RawMaterialPOId { get; set; }
        public RawMaterialPOHeader RawMaterialPO { get; set; } = default!;

        public string VehicleNumber { get; set; } = default!;

        // ── Cross-module FK columns — NO DB constraint, populated via lookup on read ──
        public int SupplierId { get; set; }       // Party (ISupplierLookup) — from PO
        public int StationId { get; set; }         // Station (IStationLookup) — from PO
        public int GodownId { get; set; }          // Warehouse / godown (IWarehouseLookup)
        public int TransporterId { get; set; }     // Transport (ITransporterLookup)

        public decimal? FreightRate { get; set; }  // From PO
        public string? InvoiceGstNo { get; set; }
        public string? LrNumber { get; set; }
        public string? ContainerNo { get; set; }   // Container shipment only

        public DateTimeOffset? LorryIn { get; set; }
        public DateTimeOffset? LorryOut { get; set; }

        // ── Weighbridge ──
        public decimal GrossWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal NetWeight { get; set; }         // computed Gross − Tare (set in handler)
        public decimal PartyWeight { get; set; }       // supplier invoice weight (user input)
        public decimal WeightDifference { get; set; }  // computed Party − Net (set in handler)

        public decimal? MoisturePercentage { get; set; }

        // ── Same-module FK (MiscMaster) — DB constraint, Dapper JOIN on read ──
        public int QcStatusId { get; set; }            // Pending / Approved / Rejected
        public MiscMaster QcStatus { get; set; } = default!;

        public string? Remarks { get; set; }

        public ICollection<ArrivalDetail>? ArrivalDetails { get; set; }

        // Transient (NOT persisted) — StockLedgerRaw rows built by the command handler
        // (individual bales from the payload, or expanded from the consolidated range).
        // The command repository sets UnitId + LotNo and persists them.
        [NotMapped]
        public List<StockLedgerRaw>? StockRows { get; set; }
    }
}
