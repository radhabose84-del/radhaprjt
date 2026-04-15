using MediatR;
using SalesManagement.Application.ProformaInvoice.Dto;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoicePrintDetails
{
    public sealed record GetProformaInvoicePrintDetailsQuery(int Id) : IRequest<ProformaInvoicePrintDto?>;
}
