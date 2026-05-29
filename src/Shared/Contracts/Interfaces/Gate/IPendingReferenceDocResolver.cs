using Contracts.Dtos.Common;

namespace Contracts.Interfaces.Gate
{
    /// <summary>
    /// Strategy interface for the centralized Gate Inward Entry screen.
    /// One implementation per <see cref="DocumentTypeCode"/>, registered in the owning module.
    /// </summary>
    /// <remarks>
    /// <para>The natural key is <see cref="DocumentTypeCode"/> — it matches
    /// <c>Finance.TransactionTypeMaster.ShortName</c>. The integer Id and human-readable name
    /// live in that table and are resolved at runtime via <c>ITransactionTypeLookup</c>; resolvers
    /// must not hardcode them.</para>
    /// <para>Two methods power the two screens:
    /// <see cref="GetPendingAsync"/> populates the "Reference Document No." multi-select dropdown
    /// (header-only), and <see cref="GetPendingItemsAsync"/> returns the line items for the POs
    /// the user picked from that dropdown.</para>
    /// </remarks>
    public interface IPendingReferenceDocResolver
    {
        /// <summary>
        /// Stable short code identifying this resolver — matches
        /// <c>Finance.TransactionTypeMaster.ShortName</c> within <c>ModuleId = 21</c> (Purchase).
        /// Examples: "LPO", "IPO", "CPO", "EPO", "BPO".
        /// </summary>
        string DocumentTypeCode { get; }

        /// <summary>
        /// Lists pending PO headers for the dropdown — approved status, qty remaining,
        /// scoped to the given party + unit.
        /// </summary>
        Task<IReadOnlyList<PendingReferenceDocDto>> GetPendingAsync(
            int partyId, int unitId, CancellationToken ct = default);

        /// <summary>
        /// Returns per-PO line items for the supplied PO ids. Only items with remaining
        /// receivable qty (OrderQty − GRN'd qty &gt; 0) are returned. Item name / UOM / tolerances
        /// are filled by the orchestrating handler via <c>IItemPurchaseToleranceLookup</c>.
        /// </summary>
        Task<IReadOnlyList<PendingReferenceDocLineDto>> GetPendingItemsAsync(
            IEnumerable<int> docIds, int partyId, int unitId, CancellationToken ct = default);
    }
}
