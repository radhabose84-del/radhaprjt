using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.MiscMaster.Commands.DeleteMiscMaster
{
    public class DeleteMiscMasterCommandHandler : IRequestHandler<DeleteMiscMasterCommand, bool>
    {
        private readonly IMiscMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteMiscMasterCommandHandler(
            IMiscMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMiscMasterCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Misc Master not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "MISC_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"MiscMaster with Id {request.Id} soft-deleted successfully.",
                module: "MiscMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
