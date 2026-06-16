using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
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
        private readonly IMediator _mediator;

        public SubmitLinkageChangeRequestCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            ITaxCodeQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(SubmitLinkageChangeRequestCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            // Modifying TaxCodeId/ControlAccountId for a GL account goes to approval (PENDING).
            var pendingStatusId = await _queryRepository.GetMiscIdAsync("ApprovalStatus", "PENDING")
                ?? throw new ExceptionRules("ApprovalStatus 'PENDING' is not configured in MiscMaster.");

            // Create a new PENDING linkage row for the requested change.
            var entity = new Domain.Entities.TaxAccountLinkage
            {
                CompanyId = companyId,
                TaxCodeId = request.NewTaxCodeId,
                GlAccountId = request.GlAccountId,
                ControlAccountId = request.NewControlAccountId,
                StatusId = pendingStatusId,
                EffectiveFrom = request.EffectiveFrom,
                ChangeReason = request.Reason,
                IsActive = Status.Inactive   // not active until approved (then /activate flips it + closes the prior row)
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
