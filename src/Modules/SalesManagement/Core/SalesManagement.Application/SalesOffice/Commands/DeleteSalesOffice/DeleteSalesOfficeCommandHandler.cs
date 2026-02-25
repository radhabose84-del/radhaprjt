#nullable disable
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOffice.Commands.DeleteSalesOffice
{
    public sealed class DeleteSalesOfficeCommandHandler : IRequestHandler<DeleteSalesOfficeCommand, bool>
    {
        private readonly ISalesOfficeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesOfficeCommandHandler(
            ISalesOfficeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesOfficeCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_OFFICE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Office with Id {request.Id} soft deleted.",
                module: "SalesOffice"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
