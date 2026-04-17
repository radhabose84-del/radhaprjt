using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetSalesOrderInvoices
{
    public class GetSalesOrderInvoicesQuery : IRequest<List<SalesOrderInvoiceDto>>
    {
        public int SalesOrderId { get; set; }
    }
}
