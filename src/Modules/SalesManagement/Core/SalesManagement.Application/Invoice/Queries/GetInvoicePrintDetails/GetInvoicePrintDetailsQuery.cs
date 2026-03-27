using MediatR;
using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Application.Invoice.Queries.GetInvoicePrintDetails
{
    public sealed record GetInvoicePrintDetailsQuery(int Id) : IRequest<InvoicePrintDto?>;
}
