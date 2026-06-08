using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Commands.SubmitFreightRfqForApproval
{
    public class SubmitFreightRfqForApprovalCommandHandler : IRequestHandler<SubmitFreightRfqForApprovalCommand, ApiResponseDTO<int>>
    {
        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public SubmitFreightRfqForApprovalCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(SubmitFreightRfqForApprovalCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SubmitForApprovalAsync(
                request.FreightRfqId, request.SelectedQuotationId, request.IsOverride, request.ComparisonRemarks);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Submit",
                actionCode: "FREIGHTRFQ_SUBMIT",
                actionName: request.FreightRfqId.ToString(),
                details: $"Freight RFQ {request.FreightRfqId} submitted for approval (selected quotation {request.SelectedQuotationId}).",
                module: "FreightRfq"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Freight RFQ submitted for approval.",
                Data = result
            };
        }
    }
}
