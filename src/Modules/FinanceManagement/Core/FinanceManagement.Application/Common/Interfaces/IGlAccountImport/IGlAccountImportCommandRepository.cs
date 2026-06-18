using FinanceManagement.Application.GlAccountImport.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IGlAccountImport
{
    public interface IGlAccountImportCommandRepository
    {
        /// <summary>
        /// Persists one import run in a single transaction: creates the new groups (parent-before-child,
        /// maintaining Level/IsLeaf), inserts accounts as <b>Inactive</b> (AC3) stamped with the new
        /// log id, then writes the log header + row-error report. Returns the new import-log id.
        /// </summary>
        Task<int> CommitAsync(GlAccountImportCommitRequest request, CancellationToken ct);

        /// <summary>
        /// Activates every (still-inactive, non-deleted) account created by the given import batch
        /// for the company. Returns the number of accounts activated (AC3 bulk activate).
        /// </summary>
        Task<int> ActivateBatchAsync(int importLogId, int companyId, CancellationToken ct);
    }
}
