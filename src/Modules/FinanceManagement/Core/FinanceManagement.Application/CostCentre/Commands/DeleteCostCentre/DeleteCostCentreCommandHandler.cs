using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CostCentre.Commands.DeleteCostCentre
{
    public class DeleteCostCentreCommandHandler : IRequestHandler<DeleteCostCentreCommand, bool>
    {
        private readonly ICostCentreCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteCostCentreCommandHandler(
            ICostCentreCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteCostCentreCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "COST_CENTRE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Cost Centre with Id {request.Id} soft deleted.",
                module: "CostCentre"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
