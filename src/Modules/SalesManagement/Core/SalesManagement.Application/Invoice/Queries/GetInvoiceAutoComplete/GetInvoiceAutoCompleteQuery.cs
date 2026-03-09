using MediatR;
using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Application.Invoice.Queries.GetInvoiceAutoComplete
{
    public sealed record GetInvoiceAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<InvoiceLookupDto>>;
}
