using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IProcurementType;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ProcurementType.Commands.DeleteProcurementType
{
    public class DeleteProcurementTypeCommandHandler : IRequestHandler<DeleteProcurementTypeCommand, bool>
    {
        private readonly IProcurementTypeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteProcurementTypeCommandHandler(
            IProcurementTypeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteProcurementTypeCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("ProcurementType not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "PROCUREMENTTYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"ProcurementType with Id {request.Id} soft deleted successfully.",
                module: "ProcurementType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
