namespace Contracts.Dtos.Lookups.Purchase
{
    /// <summary>
    /// Read-side projection of a cotton-quality parameter captured on the OCR Entry
    /// behind an Arrival, for QC inspection display. Carries Purchase-owned data only —
    /// <see cref="ParamId"/> references QC.QualityParameter and its name is resolved by
    /// the consuming (QC) module (no cross-module JOINs).
    /// </summary>
    public sealed class OcrQualityParameterLookupDto
    {
        public int ParamId { get; set; }
        public string? Value { get; set; }
    }
}
