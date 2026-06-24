using FinanceManagement.Application.AccountAuditTrail.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountAuditTrail.Queries.GetAccountAuditHistory
{
    // Per-account chronological field-level history (US-GL02-09 AC-3). Company from token.
    public sealed record GetAccountAuditHistoryQuery(string EntityName, int EntityId)
        : IRequest<List<AccountAuditTrailDto>>;
}
