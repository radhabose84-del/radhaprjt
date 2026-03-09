using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoReceipt.Commands.DeleteStoReceipt
{
    public class DeleteStoReceiptCommandHandler : IRequestHandler<DeleteStoReceiptCommand, bool>
    {
        private readonly IStoReceiptCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;

        public DeleteStoReceiptCommandHandler(
            IStoReceiptCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteStoReceiptCommand request, CancellationToken cancellationToken)
        {
            // Resolve Dispatched and Reserved status IDs for StockLedger reversal
            var dispatchedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Dispatched);
            var dispatchedStatusId = dispatchedStatus?.Id ?? 0;

            var reservedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Reserved);
            var reservedStatusId = reservedStatus?.Id ?? 0;

            // SoftDelete header and reverse StockLedger (Dispatched -> Reserved)
            var result = await _commandRepository.SoftDeleteAsync(
                request.Id, dispatchedStatusId, reservedStatusId, cancellationToken);

            if (!result)
                throw new ExceptionRules("STO Receipt not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "STORECEIPT_DELETE",
                actionName: request.Id.ToString(),
                details: $"STO Receipt with Id {request.Id} soft deleted. StockLedger reverted from Dispatched to Reserved.",
                module: "StoReceipt");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
