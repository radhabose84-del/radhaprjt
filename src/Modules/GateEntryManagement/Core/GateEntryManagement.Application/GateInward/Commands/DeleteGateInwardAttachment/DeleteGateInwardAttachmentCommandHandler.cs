using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Commands.DeleteGateInwardAttachment
{
    public class DeleteGateInwardAttachmentCommandHandler
        : IRequestHandler<DeleteGateInwardAttachmentCommand, bool>
    {
        private readonly IGateInwardCommandRepository _commandRepository;
        private readonly IGateInwardAttachmentFileStorage _storage;
        private readonly IMediator _mediator;

        public DeleteGateInwardAttachmentCommandHandler(
            IGateInwardCommandRepository commandRepository,
            IGateInwardAttachmentFileStorage storage,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _storage = storage;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteGateInwardAttachmentCommand request, CancellationToken ct)
        {
            var oldPath = await _commandRepository.ClearAttachmentAsync(request.GateInwardHdrId, ct);
            if (oldPath == null)
                return false;

            await _storage.DeleteAsync(oldPath, ct);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "GATEINWARD_ATTACHMENT_DELETE",
                actionName: request.GateInwardHdrId.ToString(),
                details: $"Gate Inward {request.GateInwardHdrId} attachment deleted.",
                module: "GateInward");

            await _mediator.Publish(auditEvent, ct);

            return true;
        }
    }
}
