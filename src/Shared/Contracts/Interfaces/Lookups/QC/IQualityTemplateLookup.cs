using Contracts.Dtos.Lookups.QC;

namespace Contracts.Interfaces.Lookups.QC
{
    /// <summary>
    /// Cross-module lookup against the QC quality-template masters
    /// (Qc.QualityTemplate / Qc.QualityTemplateParameter / Qc.QualityParameter).
    /// Consumed by PurchaseManagement (OCR Entry) to select a template, render its
    /// parameters dynamically and resolve names on read. Only IsActive = 1 /
    /// IsDeleted = 0 rows are returned. Auto-cached by AddLookupCaching().
    /// </summary>
    public interface IQualityTemplateLookup
    {
        /// <summary>Template headers (Code/Name) for a batch of template ids.</summary>
        Task<IReadOnlyList<QualityTemplateLookupDto>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default);

        /// <summary>Active parameters defined on a template, ordered by SequenceNo — drives the dynamic UI.</summary>
        Task<IReadOnlyList<QualityTemplateParameterLookupDto>> GetParametersByTemplateIdAsync(
            int qualityTemplateId, CancellationToken ct = default);

        /// <summary>Parameter rows (Code/Name/Unit) for a batch of QualityParameter ids — read-back / validation.</summary>
        Task<IReadOnlyList<QualityTemplateParameterLookupDto>> GetParametersByIdsAsync(
            IEnumerable<int> qualityParameterIds, CancellationToken ct = default);
    }
}
