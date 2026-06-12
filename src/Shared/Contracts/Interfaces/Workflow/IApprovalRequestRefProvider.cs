using Contracts.Dtos.Workflow;

namespace Contracts.Interfaces.Workflow
{
    /// <summary>
    /// Resolves workflow ApprovalRequest ids for a set of module transactions. Intentionally NOT named
    /// "*Lookup" so the global lookup cache does not wrap it — approval requests are transactional and
    /// must be read fresh (a just-created request would otherwise be hidden by a cached empty result).
    /// </summary>
    public interface IApprovalRequestRefProvider
    {
        Task<IReadOnlyList<ApprovalRequestRefDto>> GetByModuleAsync(
            string workflowType, IEnumerable<int> moduleTransactionIds, CancellationToken ct = default);
    }
}
