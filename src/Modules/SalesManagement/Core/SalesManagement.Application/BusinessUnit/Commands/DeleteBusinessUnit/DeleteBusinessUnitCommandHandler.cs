using MediatR;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.BusinessUnit.Commands.DeleteBusinessUnit
{
    public class DeleteBusinessUnitCommandHandler : IRequestHandler<DeleteBusinessUnitCommand, bool>
    {
        private readonly IBusinessUnitCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteBusinessUnitCommandHandler(
            IBusinessUnitCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteBusinessUnitCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "BUSINESSUNIT_DELETE",
                actionName: request.Id.ToString(),
                details: $"Business Unit with Id {request.Id} soft deleted.",
                module: "BusinessUnit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
