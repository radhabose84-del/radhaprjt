using MediatR;
using SalesManagement.Application.Common.Interfaces.ILotMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.LotMaster.Commands.DeleteLotMaster
{
    public class DeleteLotMasterCommandHandler : IRequestHandler<DeleteLotMasterCommand, bool>
    {
        private readonly ILotMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteLotMasterCommandHandler(
            ILotMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteLotMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "LOTMASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"LotMaster with Id {request.Id} soft deleted.",
                module: "LotMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
