using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetAllFreightRfq
{
    public class GetAllFreightRfqQuery : IRequest<ApiResponseDTO<List<FreightRfqListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public int? StatusId { get; set; }   // optional grid filter (e.g. pending-for-approval view)
    }
}
