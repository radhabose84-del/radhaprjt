using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Commands.DeleteDispatchAdvice
{
    public class DeleteDispatchAdviceCommandHandler : IRequestHandler<DeleteDispatchAdviceCommand, bool>
    {
        private readonly IDispatchAdviceCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;

        public DeleteDispatchAdviceCommandHandler(
            IDispatchAdviceCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteDispatchAdviceCommand request, CancellationToken cancellationToken)
        {
            // Resolve Reserved and Packed status IDs for StockLedger reversal
            var reservedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Reserved);
            var reservedStatusId = reservedStatus?.Id ?? 0;

            var packedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.StockStatus, MiscEnumEntity.Packed);
            var packedStatusId = packedStatus?.Id ?? 0;

            // SoftDelete header (IsDeleted=1) and reverse StockLedger (Reserved -> Packed)
            var result = await _commandRepository.SoftDeleteAsync(
                request.Id, reservedStatusId, packedStatusId, cancellationToken);

            if (!result)
                throw new ExceptionRules("Dispatch Advice not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "DISPATCHADVICE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Dispatch Advice with Id {request.Id} soft deleted. StockLedger reverted from Reserved to Packed.",
                module: "DispatchAdvice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
