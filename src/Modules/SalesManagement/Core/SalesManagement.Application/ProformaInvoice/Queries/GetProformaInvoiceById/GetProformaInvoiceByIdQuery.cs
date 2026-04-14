using MediatR;
using SalesManagement.Application.ProformaInvoice.Dto;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceById
{
    public class GetProformaInvoiceByIdQuery : IRequest<ProformaInvoiceDto?>
    {
        public int Id { get; set; }
    }
}
