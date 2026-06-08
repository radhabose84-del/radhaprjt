using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Commands.RejectFreightRfq
{
    public class RejectFreightRfqCommand : IRequest<ApiResponseDTO<int>>
    {
        public int FreightRfqId { get; set; }
        public string? ApprovalRemarks { get; set; }
    }
}
