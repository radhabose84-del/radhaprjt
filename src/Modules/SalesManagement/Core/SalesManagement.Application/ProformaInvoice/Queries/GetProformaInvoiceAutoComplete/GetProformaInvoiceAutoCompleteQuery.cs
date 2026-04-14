using MediatR;
using SalesManagement.Application.ProformaInvoice.Dto;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceAutoComplete;

public sealed record GetProformaInvoiceAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<ProformaInvoiceLookupDto>>;
