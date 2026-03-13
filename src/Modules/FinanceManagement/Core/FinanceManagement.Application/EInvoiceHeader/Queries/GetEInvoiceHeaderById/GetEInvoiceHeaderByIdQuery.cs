using FinanceManagement.Application.EInvoiceHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderById
{
    public class GetEInvoiceHeaderByIdQuery : IRequest<EInvoiceHeaderDto?>
    {
        public int Id { get; set; }
    }
}
