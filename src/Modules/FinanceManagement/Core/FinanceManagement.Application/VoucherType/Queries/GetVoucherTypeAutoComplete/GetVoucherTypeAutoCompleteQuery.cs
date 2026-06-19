using FinanceManagement.Application.VoucherType.Dto;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeAutoComplete
{
    // CompanyId is resolved from the session token in the handler.
    public sealed record GetVoucherTypeAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<VoucherTypeLookupDto>>;
}
