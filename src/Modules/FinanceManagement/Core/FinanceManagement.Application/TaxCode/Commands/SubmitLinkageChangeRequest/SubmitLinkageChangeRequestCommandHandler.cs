using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest
{
    public class SubmitLinkageChangeRequestCommandHandler : IRequestHandler<SubmitLinkageChangeRequestCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;

        public SubmitLinkageChangeRequestCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            ITaxCodeQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(SubmitLinkageChangeRequestCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            // Modifying TaxCodeId/ControlAccountId for a GL account goes to approval (PENDING).
            var pendingStatusId = await _queryRepository.GetMiscIdAsync("ApprovalStatus", "PENDING")
                ?? throw new ExceptionRules("ApprovalStatus 'PENDING' is not configured in MiscMaster.");

            // Point the change-request row at the GL account's current active linkage; old tax code /
            // control account are read from that row for the Change-Audit "Old" columns.
            var current = await _queryRepository.GetLinkageByAccountAsync(request.GlAccountId);

            // Create a new PENDING linkage row for the requested change.
            var entity = new Domain.Entities.TaxAccountLinkage
            {
                CompanyId = companyId,
                TaxCodeId = request.NewTaxCodeId,
                GlAccountId = request.GlAccountId,
                ControlAccountId = request.NewControlAccountId,
                OldTaxLinkageId = current?.Id,
                StatusId = pendingStatusId,
                EffectiveFrom = request.EffectiveFrom,
                ChangeReason = request.Reason,
                IsActive = Status.Inactive   // not active until approved (then /activate flips it + closes the prior row)
            };

            var newId = await _commandRepository.CreateLinkageAsync(entity);

            // Raise the dual-approval (FC + Tax Lead) request via the SQL transactional outbox.
            // The BackgroundService Workflow module consumes it; on approval-complete it sends
            // ActivateTaxAccountLinkageCommand which flips this row to APPROVED + active and closes
            // the prior active row. ModuleTypeName MUST equal the configured Menu / WorkflowType name.
            // Build the workflow payload from the persisted row (mirrors CreateSalesOrderCommandHandler):
            // fetch the full linkage as the Header, wrap in the reverse-map DTO, serialize.
            var workFlowEntity = await _queryRepository.GetLinkageByIdAsync(newId);
            if (workFlowEntity != null)
            {
                // sp_EvaluateApproval reads $.Header.UnitId (AppData.ApprovalRequest.UnitId is NOT NULL).
                // The linkage has no UnitId column, so supply the acting user's unit from the token.
                workFlowEntity.UnitId = _ipAddressService.GetUnitId()
                    ?? throw new ExceptionRules("No active unit in session — required for the approval workflow.");
            }

            var reverseMap = new SubmitLinkageChangeRequestReverseDto
            {
                Header = workFlowEntity,
                Lines = null
            };
            string serializedPayload = JsonSerializer.Serialize(reverseMap);

            var correlationId = Guid.NewGuid();
            var approvalRequest = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.TaxAccountLinkage,
                ModuleTransactionId = newId,
                Payload = serializedPayload                
            };
            await _outboxEventPublisher.ScheduleAsync(approvalRequest, correlationId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "ChangeRequest",
                actionCode: "TAX_ACCOUNT_LINKAGE_CHANGE_REQUEST",
                actionName: request.GlAccountId.ToString(),
                details: $"Linkage change requested for GlAccountId {request.GlAccountId}: tax code -> {request.NewTaxCodeId}. Reason: {request.Reason}",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Linkage change request submitted for dual approval.",
                Data = newId
            };
        }
    }
}
