using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Commands.ApproveFreightRfq
{
    public class ApproveFreightRfqCommandHandler : IRequestHandler<ApproveFreightRfqCommand, ApiResponseDTO<int>>
    {
        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public ApproveFreightRfqCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(ApproveFreightRfqCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.ApproveAsync(request.FreightRfqId, request.ApprovalRemarks);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Approve",
                actionCode: "FREIGHTRFQ_APPROVE",
                actionName: request.FreightRfqId.ToString(),
                details: $"Freight RFQ {request.FreightRfqId} approved.",
                module: "FreightRfq"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Freight RFQ approved successfully.",
                Data = result
            };
        }
    }
}
