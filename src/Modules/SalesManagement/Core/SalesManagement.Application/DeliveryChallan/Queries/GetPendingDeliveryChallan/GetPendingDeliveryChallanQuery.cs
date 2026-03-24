using Contracts.Common;
using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallan
{
    public class GetPendingDeliveryChallanQuery : IRequest<ApiResponseDTO<List<DeliveryChallanHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
