#nullable disable
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster
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
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "MISC_TYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Misc Type Master with Id {request.Id} soft deleted.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
