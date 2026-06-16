using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest
{
    public class SubmitLinkageChangeRequestCommandHandler : IRequestHandler<SubmitLinkageChangeRequestCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public SubmitLinkageChangeRequestCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(SubmitLinkageChangeRequestCommand request, CancellationToken cancellationToken)
        {
            // Create a new PENDING linkage row for the requested code change.
            var entity = new Domain.Entities.TaxAccountLinkage
            {
                CompanyId = request.CompanyId,
                TaxCodeId = request.NewTaxCodeId,
                GlAccountId = request.GlAccountId,
                ApprovalStatus = "PENDING",
                IsActivated = false,
                EffectiveFrom = request.EffectiveFrom,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var newId = await _commandRepository.CreateLinkageAsync(entity);

            // TODO: raise an ApprovalRequest via the BackgroundService Workflow module
            // (FC + Tax Lead dual approval). On approval-complete the workflow callback
            // flips this row to APPROVED + IsActivated and closes the prior APPROVED row.

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
