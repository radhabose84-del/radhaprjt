using FinanceManagement.Application.EInvoiceHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderAutoComplete
{
    public sealed record GetEInvoiceHeaderAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<EInvoiceHeaderLookupDto>>;
}
