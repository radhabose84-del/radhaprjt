using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RawMaterialType.Commands.DeleteRawMaterialType
{
    public class DeleteRawMaterialTypeCommandHandler : IRequestHandler<DeleteRawMaterialTypeCommand, bool>
    {
        private readonly IRawMaterialTypeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteRawMaterialTypeCommandHandler(
            IRawMaterialTypeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteRawMaterialTypeCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "RAWMATERIALTYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Raw Material Type with Id {request.Id} soft deleted.",
                module: "RawMaterialType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
