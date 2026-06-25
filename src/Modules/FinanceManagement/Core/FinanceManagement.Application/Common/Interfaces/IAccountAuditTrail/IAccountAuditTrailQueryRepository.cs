using FinanceManagement.Application.AccountAuditTrail.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IAccountAuditTrail
{
    // Read-only access to the immutable Finance.AccountAuditTrail (US-GL02-09).
    public interface IAccountAuditTrailQueryRepository
    {
        // Per-account, field-level history in chronological order (AC-3).
        Task<List<AccountAuditTrailDto>> GetHistoryAsync(
            int companyId, string entityName, int entityId, CancellationToken ct);

        // All audit rows for a company within [from, to), optionally filtered to one entity type (AC-4).
        Task<List<AccountAuditTrailDto>> ExportAsync(
            int companyId, DateTimeOffset from, DateTimeOffset to, string? entityName, CancellationToken ct);
    }
}
