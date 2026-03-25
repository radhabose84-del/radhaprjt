using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetCustomerInvoices
{
    public class GetCustomerInvoicesQuery : IRequest<List<CustomerInvoiceDto>>
    {
        public int CustomerId { get; set; }
    }
}
