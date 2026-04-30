using MediatR;
using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Application.Invoice.Queries.GetInvoicePrintDetailsByIds
{
    public class GetInvoicePrintDetailsByIdsQuery : IRequest<List<InvoicePrintDto>>
    {
        public List<int> InvoiceIds { get; set; } = [];
    }
}
