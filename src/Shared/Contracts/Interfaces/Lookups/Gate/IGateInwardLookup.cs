using Contracts.Dtos.Lookups.Gate;

namespace Contracts.Interfaces.Lookups.Gate
{
    /// <summary>
    /// Cross-module lookup for resolving centralized Gate Inward header info
    /// (<c>GateEntryNo</c>, <c>GateEntryDate</c>) by id. Implementation lives in
    /// <c>GateEntryManagement.Infrastructure</c>.
    /// </summary>
    /// <remarks>
    /// Auto-cached via <c>CachedLookupDecorator</c> (30 min sliding / 24 h absolute) because the
    /// type name ends in <c>Lookup</c> — no per-call DI plumbing required.
    /// </remarks>
    public interface IGateInwardLookup
    {
        Task<IReadOnlyList<GateInwardLookupDto>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default);
    }
}
