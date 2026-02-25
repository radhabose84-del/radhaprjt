
using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesSegment.Dto;

namespace SalesManagement.Application.SalesSegment.Queries.GetAllSalesSegment
{
    public class GetAllSalesSegmentQuery : IRequest<ApiResponseDTO<List<SalesSegmentDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
