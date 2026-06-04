namespace Contracts.Dtos.Lookups.QC
{
    /// <summary>
    /// Result of a Qc.QualitySpecification existence lookup keyed by ItemId and/or ItemCategoryId.
    /// Each set lists only those IDs for which at least one active, in-effect specification was found.
    /// </summary>
    public sealed class QualitySpecificationMatchDto
    {
        public HashSet<int> MatchedItemIds { get; init; } = new();
        public HashSet<int> MatchedItemCategoryIds { get; init; } = new();
    }
}
