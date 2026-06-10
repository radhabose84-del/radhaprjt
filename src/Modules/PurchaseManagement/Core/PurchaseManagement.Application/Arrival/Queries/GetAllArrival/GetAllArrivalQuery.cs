using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Arrival.Dto;

namespace PurchaseManagement.Application.Arrival.Queries.GetAllArrival
{
    public class GetAllArrivalQuery : IRequest<ApiResponseDTO<List<ArrivalDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
