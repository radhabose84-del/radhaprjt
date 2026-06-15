using Contracts.Dtos.Lookups.Purchase;

namespace Contracts.Interfaces.Lookups.Purchase
{
    /// <summary>
    /// Cross-module read access to the cotton-quality parameters captured on the OCR Entry
    /// behind an Arrival (ArrivalHeader → RawMaterialPOHeader → OCREntry → OCRQualityParameter),
    /// scoped to that exact OCR. Used by QC inspection to display the originally-entered
    /// OCR parameter values alongside the inspection results.
    /// </summary>
    public interface IOcrQualityParameterLookup
    {
        /// <summary>
        /// OCR quality parameter values (ParamId + Value) for the OCR linked to the given
        /// Arrival header. Returns an empty list when the Arrival has no PO/OCR chain or no parameters.
        /// </summary>
        Task<IReadOnlyList<OcrQualityParameterLookupDto>> GetByArrivalHeaderIdAsync(
            int arrivalHeaderId, CancellationToken ct = default);
    }
}
