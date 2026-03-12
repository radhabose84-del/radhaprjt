using Contracts.Common;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetAllEInvoiceHeader
{
    public class GetAllEInvoiceHeaderQuery : IRequest<ApiResponseDTO<List<EInvoiceHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
