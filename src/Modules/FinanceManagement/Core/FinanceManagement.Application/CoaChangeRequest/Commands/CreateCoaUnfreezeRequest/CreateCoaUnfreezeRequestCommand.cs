using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaUnfreezeRequest
{
    // US-GL02-08B — open a dual-approval unfreeze request batching one or more impact-approved change
    // requests (AC5). Only change requests whose impact the CFO has approved may be attached.
    public class CreateCoaUnfreezeRequestCommand : IRequest<ApiResponseDTO<int>>
    {
        public string Reason { get; set; } = null!;
        public List<int> ChangeRequestIds { get; set; } = new();
        public int? WindowMinutes { get; set; }
    }
}
