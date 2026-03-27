using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Commands.DeleteGatePass
{
    public class DeleteGatePassCommandHandler : IRequestHandler<DeleteGatePassCommand, bool>
    {
        private readonly IGatePassCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMediator _mediator;

        public DeleteGatePassCommandHandler(
            IGatePassCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteGatePassCommand request, CancellationToken cancellationToken)
        {
            // Resolve VMR IN status
            var inStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.VMRStatus, MiscEnumEntity.VMRStatusInsidePremises);
            var vmrInStatusId = inStatus?.Id
                ?? throw new ExceptionRules("VMR Status 'IN' not found in MiscMaster.");

            var result = await _commandRepository.SoftDeleteAsync(request.Id, vmrInStatusId, cancellationToken);

            if (!result)
                throw new ExceptionRules("Gate Pass not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "GATEPASS_DELETE",
                actionName: request.Id.ToString(),
                details: $"Gate Pass with Id {request.Id} soft-deleted successfully. VMR reverted to IN, document GEFlags reverted to 0.",
                module: "GatePass"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
