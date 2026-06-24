using FinanceManagement.Application.AccountAuditTrail.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountAuditTrail.Queries.ExportAccountAudit
{
    // Export audit rows for a date range with a tamper-evident checksum (US-GL02-09 AC-4).
    // Company from token. EntityName optional (null = all audited COA entities). [From, To) half-open.
    public sealed record ExportAccountAuditQuery(DateTimeOffset From, DateTimeOffset To, string? EntityName)
        : IRequest<AccountAuditExportDto>;
}
