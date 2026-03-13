using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.MiscMaster.Commands.DeleteMiscMaster
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
                details: $"Misc Master with Id {request.Id} deleted successfully.",
                module: "MiscMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
