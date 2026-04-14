using Contracts.Common;
using MediatR;
using SalesManagement.Application.ProformaInvoice.Dto;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetAllProformaInvoice
{
    public class GetAllProformaInvoiceQuery : IRequest<ApiResponseDTO<List<ProformaInvoiceDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
