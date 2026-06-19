using FinanceManagement.Application.VoucherType.Dto;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeSummary
{
    // CompanyId is resolved from the session token in the handler.
    public sealed record GetVoucherTypeSummaryQuery
        : IRequest<VoucherTypeSummaryDto>;
}
