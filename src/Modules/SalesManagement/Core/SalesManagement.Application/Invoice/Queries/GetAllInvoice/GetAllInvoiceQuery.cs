using Contracts.Common;
using MediatR;
using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Application.Invoice.Queries.GetAllInvoice
{
    public class GetAllInvoiceQuery : IRequest<ApiResponseDTO<List<InvoiceHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
    }
}
