using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetInvoiceLineDetails
{
    public class GetInvoiceLineDetailsQuery : IRequest<List<InvoiceLineDetailDto>>
    {
        public int InvoiceHeaderId { get; set; }
    }
}
