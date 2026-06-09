using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Commands.SubmitFreightRfqForApproval
{
    public class SubmitFreightRfqForApprovalCommandHandler : IRequestHandler<SubmitFreightRfqForApprovalCommand, ApiResponseDTO<int>>
    {
        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;

        public SubmitFreightRfqForApprovalCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IMediator mediator,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
        }

        public async Task<ApiResponseDTO<int>> Handle(SubmitFreightRfqForApprovalCommand request, CancellationToken cancellationToken)
        {
            // Mark the selected transporter + move status to Pending Approval (local state).
            var result = await _commandRepository.SubmitForApprovalAsync(
                request.FreightRfqId, request.SelectedQuotationId, request.IsOverride, request.ComparisonRemarks);

            // Raise the approval request into the centralized Workflow engine (via outbox).
            // ModuleTypeName "Freight RFQ" matches the Workflow menu name, so the engine resolves the
            // WorkflowType by menu name (GetMenuIdByNameAsync) — no Finance.TransactionTypeMaster row needed.
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var payload = await _commandRepository.GetWorkflowPayloadAsync(request.FreightRfqId);
            if (payload != null)
                payload.UnitId = unitId;

            // sp_EvaluateApproval reads $.Header.UnitId / $.Lines, so the payload must be wrapped as
            // { "Header": {...}, "Lines": null } — mirroring Blanket Master. No TransactionType for Freight RFQ.
            var reverse = new FreightRfqWorkflowReverseDto { Header = payload, Lines = null };

            var correlationId = Guid.NewGuid();
            var workflowCommand = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.TransactionTypeFreightRfq,
                ModuleTransactionId = request.FreightRfqId,
                Payload = JsonSerializer.Serialize(reverse)
            };
            await _outboxEventPublisher.ScheduleAsync(workflowCommand, correlationId, cancellationToken);

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
