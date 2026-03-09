using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Commands.DeleteDeliveryChallan
{
    public class DeleteDeliveryChallanCommandHandler : IRequestHandler<DeleteDeliveryChallanCommand, bool>
    {
        private readonly IDeliveryChallanCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;

        public DeleteDeliveryChallanCommandHandler(
            IDeliveryChallanCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteDeliveryChallanCommand request, CancellationToken cancellationToken)
        {
            // Resolve Reserved and Packed status IDs for StockLedger reversal
            var reservedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Reserved);
            var reservedStatusId = reservedStatus?.Id ?? 0;

            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var packedStatusId = packedStatus?.Id ?? 0;

            // SoftDelete header and reverse StockLedger (Reserved -> Packed)
            var result = await _commandRepository.SoftDeleteAsync(
                request.Id, reservedStatusId, packedStatusId, cancellationToken);

            if (!result)
                throw new ExceptionRules("Delivery Challan not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "DELIVERYCHALLAN_DELETE",
                actionName: request.Id.ToString(),
                details: $"Delivery Challan with Id {request.Id} soft deleted. StockLedger reverted from Reserved to Packed.",
                module: "DeliveryChallan");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
