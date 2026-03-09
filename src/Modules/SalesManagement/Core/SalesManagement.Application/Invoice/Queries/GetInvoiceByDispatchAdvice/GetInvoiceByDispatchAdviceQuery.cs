using Contracts.Common;
using MediatR;
using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Application.Invoice.Queries.GetInvoiceByDispatchAdvice
{
    public class GetInvoiceByDispatchAdviceQuery : IRequest<ApiResponseDTO<List<InvoiceHeaderDto>>>
    {
        public int DispatchAdviceId { get; set; }
    }
}
