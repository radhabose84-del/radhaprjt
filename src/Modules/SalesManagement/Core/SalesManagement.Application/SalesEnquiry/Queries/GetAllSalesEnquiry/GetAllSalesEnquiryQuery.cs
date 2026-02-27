using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesEnquiry.Dto;

namespace SalesManagement.Application.SalesEnquiry.Queries.GetAllSalesEnquiry
{
    public class GetAllSalesEnquiryQuery : IRequest<ApiResponseDTO<List<SalesEnquiryHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
