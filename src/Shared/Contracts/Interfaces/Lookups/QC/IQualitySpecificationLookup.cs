using Contracts.Dtos.Lookups.QC;

namespace Contracts.Interfaces.Lookups.QC
{
    /// <summary>
    /// Cross-module lookup against Qc.QualitySpecification used by consumers (e.g. Purchase GRN)
    /// to flag rows where a quality template applies by either ItemId or ItemCategoryId.
    /// Only IsActive = 1 and IsDeleted = 0 rows are considered.
    /// </summary>
    public interface IQualitySpecificationLookup
    {
        Task<QualitySpecificationMatchDto> GetMatchingAsync(
            IEnumerable<int> itemIds,
            IEnumerable<int> itemCategoryIds,
            CancellationToken ct = default);
    }
}
