using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ProfitCentre.Commands.DeleteProfitCentre
{
    public class DeleteProfitCentreCommandHandler : IRequestHandler<DeleteProfitCentreCommand, bool>
    {
        private readonly IProfitCentreCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteProfitCentreCommandHandler(
            IProfitCentreCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteProfitCentreCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "PROFIT_CENTRE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Profit Centre with Id {request.Id} soft deleted.",
                module: "ProfitCentre"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
