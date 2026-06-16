using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.ActivateTaxAccountLinkage
{
    public class ActivateTaxAccountLinkageCommandHandler : IRequestHandler<ActivateTaxAccountLinkageCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public ActivateTaxAccountLinkageCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            ITaxCodeQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(ActivateTaxAccountLinkageCommand request, CancellationToken cancellationToken)
        {
            // Approval-complete: flip the PENDING row to APPROVED + activated.
            var approvedStatusId = await _queryRepository.GetMiscIdAsync("ApprovalStatus", "APPROVED")
                ?? throw new ExceptionRules("ApprovalStatus 'APPROVED' is not configured in MiscMaster.");

            await _commandRepository.ActivateLinkageAsync(request.Id, approvedStatusId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Activate",
                actionCode: "TAX_ACCOUNT_LINKAGE_ACTIVATE",
                actionName: request.Id.ToString(),
                details: $"Tax-account linkage Id {request.Id} activated.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Tax-account linkage activated successfully.",
                Data = request.Id
            };
        }
    }
}
