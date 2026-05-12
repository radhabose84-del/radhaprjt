using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreement
{
    public class DeleteSalesAgreementCommandHandler : IRequestHandler<DeleteSalesAgreementCommand, bool>
    {
        private readonly ISalesAgreementCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;

        public DeleteSalesAgreementCommandHandler(
            ISalesAgreementCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesAgreementCommand request, CancellationToken cancellationToken)
        {
            // Resolve "Cancelled" status at runtime (Sales.MiscMaster where MiscTypeId=36/ApprovalStatus).
            var cancelledStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesAgreementApprovalStatus,
                MiscEnumEntity.SalesAgreementStatusCancelled)
                ?? throw new ExceptionRules("Cancelled status is not configured in MiscMaster (MiscTypeCode='ApprovalStatus', Code='Cancelled').");

            var result = await _commandRepository.CancelAsync(request.Id, cancelledStatus.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Sales Agreement not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Cancel",
                actionCode: "SALESAGREEMENT_CANCEL",
                actionName: request.Id.ToString(),
                details: $"Sales Agreement with Id {request.Id} cancelled (status set to Cancelled).",
                module: "SalesAgreement");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
