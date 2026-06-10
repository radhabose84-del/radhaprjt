using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.FreightRfq.Dto;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqPending
{
    /// <summary>Pending-approval Freight RFQs for the WorkFlow Approval screen.</summary>
    public class GetFreightRfqPendingQuery : IRequest<ApiResponseDTO<List<FreightRfqListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
