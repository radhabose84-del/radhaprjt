using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using Contracts.Interfaces;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestDetail
{
    public class GetApprovalRequestDetailQueryHandler
        : IRequestHandler<GetApprovalRequestDetailQuery, List<ApprovalRequestDetailDto>>
    {
        private readonly IApprovalRequestQuery _repository;
        private readonly IIPAddressService _ipAddressService;

        public GetApprovalRequestDetailQueryHandler(
            IApprovalRequestQuery repository,
            IIPAddressService ipAddressService)
        {
            _repository = repository;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<ApprovalRequestDetailDto>> Handle(
            GetApprovalRequestDetailQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _ipAddressService.GetUserId();

            var result = await _repository.GetApprovalRequestDetailAsync(
                request.ModuleTransactionId,
                request.WorkflowType,
                request.Pending,
                userId);

            return result ?? new List<ApprovalRequestDetailDto>();
        }
    }
}
