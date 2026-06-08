using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Commands.ApproveFreightRfq
{
    public class ApproveFreightRfqCommand : IRequest<ApiResponseDTO<int>>
    {
        public int FreightRfqId { get; set; }
        public string? ApprovalRemarks { get; set; }
    }
}
