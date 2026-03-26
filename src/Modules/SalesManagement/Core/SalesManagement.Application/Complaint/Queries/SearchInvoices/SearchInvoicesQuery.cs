using Contracts.Common;
using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.SearchInvoices
{
    public class SearchInvoicesQuery : IRequest<ApiResponseDTO<List<InvoiceSearchDto>>>
    {
        public int PartyId { get; set; }
        public string? SearchTerm { get; set; }
        public bool LastOneYear { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
