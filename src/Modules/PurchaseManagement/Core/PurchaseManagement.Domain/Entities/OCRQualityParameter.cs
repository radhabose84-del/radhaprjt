using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    /// <summary>
    /// A single cotton-quality parameter value captured on an OCR Entry, driven by the
    /// selected QC Quality Template. Table: [Purchase].[OCRQualityParameter].
    /// One row per template parameter; the entered <see cref="Value"/> is free text.
    /// </summary>
    public class OCRQualityParameter : BaseEntity
    {
        // ── Same-module FK (parent OCR) — DB constraint, cascade-deleted with the OCR ──
        public int OcrId { get; set; }
        public OCREntry Ocr { get; set; } = default!;

        // Denormalised: the QC template this row belongs to (cross-module, no DB constraint).
        public int QualityTemplateId { get; set; }

        // Cross-module FK → QC.QualityParameter (no DB constraint). Name resolved via lookup on read.
        public int ParamId { get; set; }

        // Free-text entered value (e.g. "29.50+ MM", "3.70 - 4.30").
        public string? Value { get; set; }
    }
}
