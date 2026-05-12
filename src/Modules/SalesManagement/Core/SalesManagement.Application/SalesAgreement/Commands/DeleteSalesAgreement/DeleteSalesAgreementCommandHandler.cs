using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreement
{
    public class DeleteSalesAgreementCommandHandler : IRequestHandler<DeleteSalesAgreementCommand, bool>
    {
        private readonly ISalesAgreementCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesAgreementCommandHandler(
            ISalesAgreementCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesAgreementCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Sales Agreement not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALESAGREEMENT_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Agreement with Id {request.Id} soft deleted.",
                module: "SalesAgreement");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
