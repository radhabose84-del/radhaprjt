using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Commands.RejectFreightRfq
{
    public class RejectFreightRfqCommandHandler : IRequestHandler<RejectFreightRfqCommand, ApiResponseDTO<int>>
    {
        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public RejectFreightRfqCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(RejectFreightRfqCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.RejectAsync(request.FreightRfqId, request.ApprovalRemarks);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Reject",
                actionCode: "FREIGHTRFQ_REJECT",
                actionName: request.FreightRfqId.ToString(),
                details: $"Freight RFQ {request.FreightRfqId} rejected.",
                module: "FreightRfq"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Freight RFQ rejected.",
                Data = result
            };
        }
    }
}
