using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.Domain.Entities.GRN.GRNEntry
{
    public class GrnDetail
    {
        public int Id { get; set; }
        public int GrnId { get; set; }
        public GrnHeader GrnHeaderDetailsMaster { get; set; } = null!;
        public int PoId { get; set; }
        public PurchaseOrderHeader GrnPoDetails { get; set; } = null!;
        public int? PoSlNoLocal { get; set; }
        public int PoCategoryId { get; set; }
        public MiscMaster? PoGrnCategoryDetails { get; set; }
        public int PoMethodId { get; set; }
        public MiscMaster? PoGrnMethodDetails { get; set; }
        public int ItemId { get; set; }
        public decimal OrderQuantity { get; set; }
        public decimal DcQuantity { get; set; }
        public decimal? UpperTolerance { get; set; }
        public decimal? LowerTolerance { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }
        public string? BatchNumber { get; set; }
        public decimal? QcAcceptedQuantity { get; set; }
        public decimal? QcRejectedQuantity { get; set; }
        public string? QcRejectedRemarks { get; set; }

        // QC sign-off — per item line (moved from GrnHeader; QC is now line-level).
        // QcStatusId is a plain cross-module reference — no navigation property, no DB FK.
        public string? QcPersonName { get; set; }
        public string? QcRemarks { get; set; }
        public int? QcStatusId { get; set; }
        public DateTimeOffset? QcDate { get; set; }
        public string? QcApprovedIp { get; set; }
        public bool IsQcApproved { get; set; }
        public decimal? ItemValue { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? CGST { get; set; }
        public decimal? SGST { get; set; }
        public decimal? IGST { get; set; }
        public decimal? GSTPercentage { get; set; }
        public int? UOMId { get; set; }
        public decimal? TaxableAmount { get; set; }
        public string? GrnDetailImage { get; set; }
        public ICollection<GrnPutAwayRule>? GrnPutAwayDetails { get; set; }
        

    }
}