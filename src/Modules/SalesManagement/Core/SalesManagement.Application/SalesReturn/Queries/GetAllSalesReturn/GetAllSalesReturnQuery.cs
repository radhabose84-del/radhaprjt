using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesReturn.Dto;

namespace SalesManagement.Application.SalesReturn.Queries.GetAllSalesReturn
{
    public class GetAllSalesReturnQuery : IRequest<ApiResponseDTO<List<SalesReturnListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public int? CustomerId { get; set; }
    }
}
