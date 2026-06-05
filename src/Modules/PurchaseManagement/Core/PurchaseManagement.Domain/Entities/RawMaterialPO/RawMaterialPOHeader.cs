using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.RawMaterialPO
{
    /// <summary>
    /// Raw-material (cotton) Purchase Order / Procurement Agreement created by converting an
    /// approved OCR. Table: [Purchase].[RawMaterialPOHeader].
    /// </summary>
    public class RawMaterialPOHeader : BaseEntity
    {
        public int UnitId { get; set; }

        // System generated, unique, immutable (DocumentSequence / TransactionType master)
        public string PONumber { get; set; } = default!;
        public DateTimeOffset PODate { get; set; }

        // ── Same-module FK (OCREntry) — DB constraint, Dapper JOIN on read ──
        public int OcrId { get; set; }
        public OCREntry Ocr { get; set; } = default!;

        // Purchase Order / Procurement Agreement (MiscMaster)
        public int ProcurementDocumentTypeId { get; set; }
        public MiscMaster ProcurementDocumentType { get; set; } = default!;

        // Conversion progress — auto-calculated (Partially / Fully Converted).
        // MiscMaster, MiscType "ConversionStatus". Navigation named ConversionStatus to avoid
        // hiding BaseEntity.Status (the enum type).
        public int StatusId { get; set; }
        public MiscMaster ConversionStatus { get; set; } = default!;

        // ── Header grand totals (aggregated from detail lines in the command handler) ──
        public decimal? TaxableTotal { get; set; }    // Σ line ItemValue
        public decimal? TotalGstAmount { get; set; }  // Σ line TotalGST
        public decimal? NetTotal { get; set; }        // Σ line NetValue

        public string? Remarks { get; set; }

        public ICollection<RawMaterialPODetail>? RawMaterialPODetails { get; set; }
    }
}
