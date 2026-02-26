using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MiscMaster.Commands.DeleteMiscMaster
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
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "MISC_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Misc Master with Id {request.Id} soft deleted.",
                module: "MiscMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
