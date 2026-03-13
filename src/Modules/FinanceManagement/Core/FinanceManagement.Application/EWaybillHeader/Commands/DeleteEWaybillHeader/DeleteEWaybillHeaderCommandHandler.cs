using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Commands.DeleteEWaybillHeader
{
    public class DeleteEWaybillHeaderCommandHandler : IRequestHandler<DeleteEWaybillHeaderCommand, bool>
    {
        private readonly IEWaybillHeaderCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteEWaybillHeaderCommandHandler(
            IEWaybillHeaderCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteEWaybillHeaderCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "EWAYBILL_HEADER_DELETE",
                actionName: request.Id.ToString(),
                details: $"EWaybill Header with Id {request.Id} soft deleted.",
                module: "EWaybillHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
