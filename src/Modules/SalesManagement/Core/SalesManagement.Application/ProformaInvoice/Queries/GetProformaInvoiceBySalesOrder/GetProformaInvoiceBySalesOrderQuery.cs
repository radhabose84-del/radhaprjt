using MediatR;
using SalesManagement.Application.ProformaInvoice.Dto;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceBySalesOrder
{
    public class GetProformaInvoiceBySalesOrderQuery : IRequest<List<ProformaInvoiceDto>>
    {
        public int SalesOrderId { get; set; }
    }
}
