using Contracts.Dtos.Lookups.Purchase;

namespace Contracts.Interfaces.Lookups.Purchase
{
    /// <summary>
    /// Cross-module read access to Purchase-owned T&amp;C templates.
    /// Resolves the active template (with TermsHtml) applicable to a given Finance
    /// transaction type — used by consuming modules (e.g. Sales invoice printing).
    /// </summary>
    public interface ITnCTemplateLookup
    {
        /// <summary>
        /// Returns the most recent active, non-deleted T&amp;C template whose applicability
        /// matches the given transaction type, or null when none applies.
        /// </summary>
        Task<TnCTemplateLookupDto?> GetByTransactionTypeAsync(int transactionTypeId, CancellationToken ct = default);
    }
}
