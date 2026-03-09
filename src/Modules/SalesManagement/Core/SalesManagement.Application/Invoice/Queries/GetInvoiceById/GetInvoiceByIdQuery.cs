using MediatR;
using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Application.Invoice.Queries.GetInvoiceById
{
    public class GetInvoiceByIdQuery : IRequest<InvoiceHeaderDto>
    {
        public int Id { get; set; }
    }
}
