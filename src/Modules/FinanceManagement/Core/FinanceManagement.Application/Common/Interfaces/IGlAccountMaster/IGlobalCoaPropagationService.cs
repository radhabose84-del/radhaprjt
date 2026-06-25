namespace FinanceManagement.Application.Common.Interfaces.IGlAccountMaster
{
    // US-GL02-10 Multi-Company COA — keeps the per-company copies of the global template in sync.
    public interface IGlobalCoaPropagationService
    {
        // AC1 — a (new) subsidiary pulls every non-restricted global template account it does not yet
        // have, as a linked copy (GlobalAccountId = template). Returns the number of copies created.
        Task<int> InheritForCompanyAsync(int targetCompanyId, CancellationToken ct);

        // AC1/AC3 — a newly-created global template account is fanned out to every other company in the
        // same entity that does not yet have it. No-op if the account is not global. Returns copies created.
        Task<int> FanOutNewGlobalAsync(int globalAccountId, CancellationToken ct);

        // AC3 — an edit to a global template account is pushed to its per-company copies, skipping any
        // copy flagged IsLocalOverride (an entity override exists). No-op if not global. Returns copies updated.
        Task<int> PropagateUpdateAsync(int globalAccountId, CancellationToken ct);
    }
}
