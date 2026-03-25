using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommandHandler : IRequestHandler<DeleteMiscTypeMasterCommand, bool>
    {
        private readonly IMiscTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteMiscTypeMasterCommandHandler(
            IMiscTypeMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Misc Type Master not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "MISC_TYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"MiscTypeMaster with Id {request.Id} soft-deleted successfully.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
