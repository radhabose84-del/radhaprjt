using PurchaseManagement.Domain.Entities.GRN.GateEntry;

namespace PurchaseManagement.Domain.Entities.GRN.GRNEntry
{
    public class GrnHeader
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? GrnNo { get; set; }
        public DateTimeOffset GrnDate { get; set; }
        // Nullable + no DB FK constraint. Legacy column once pointed at Purchase.GateEntryHeader,
        // which is deprecated. New centralized flow stores Gate.GateInwardHdr.Id here; older rows
        // may still reference Purchase.GateEntryHeader. Null is allowed for callers that don't
        // have a gate entry (e.g., direct GRN without a gate flow).
        public int? GateEntryId { get; set; }
        public GateEntryHeader? GrnHeaderDetails { get; set; }
        public int PartyId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public string? DcNo { get; set; }
        public DateTimeOffset? DcDate { get; set; }
        public int ReceivingWarehouseId { get; set; }
        public string? Remarks { get; set; }
        public bool IsGrnGenerated { get; set; }
        public string? GrnReceivedImage { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        // QC fields moved to GrnDetail — QC is now per-line.
        // Header keeps only QcWarehouseId and RejectedImage (below).
        public int? QcWarehouseId { get; set; }
        public string? RejectedImage { get; set; }
        public decimal? ItemsTotal { get; set; }
        public decimal? DiscountTotal { get; set; }
        public decimal? TaxableAmount { get; set; }
        public decimal? CGSTTotal { get; set; }
        public decimal? SGSTTotal { get; set; }
        public decimal? IGSTTotal { get; set; }
        public decimal? MiscCharges { get; set; }
        public decimal? RoundOff { get; set; }
        public decimal? PurchaseValue { get; set; }
        public ICollection<GrnDetail>? GrnDetails { get; set; }
    }
}