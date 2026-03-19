using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UsageType.Commands.DeleteUsageType
{
    public class DeleteUsageTypeCommandHandler : IRequestHandler<DeleteUsageTypeCommand, bool>
    {
        private readonly IUsageTypeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteUsageTypeCommandHandler(
            IUsageTypeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteUsageTypeCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("UsageType not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "USAGETYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"UsageType with Id {request.Id} soft deleted successfully.",
                module: "UsageType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
