using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Port.Commands.Delete
{
    public sealed class DeletePortMasterCommandHandler
        : IRequestHandler<DeletePortMasterCommand, bool>
    {
        private readonly IPortMasterCommandRepository _commandRepo;
        private readonly IPortMasterQueryRepository _queryRepo;
        private readonly IMediator _mediator;

        public DeletePortMasterCommandHandler(
            IPortMasterCommandRepository commandRepo,
            IPortMasterQueryRepository queryRepo,
            IMediator mediator)
        {
            _commandRepo = commandRepo;
            _queryRepo   = queryRepo;
            _mediator    = mediator;
        }

        public async Task<bool> Handle(DeletePortMasterCommand request, CancellationToken ct)
        {
            // fetch for audit text first (read side)
            var before = await _queryRepo.GetByIdAsync(request.Id, ct)
                         ?? throw new ExceptionRules("Port Master not found.");

            var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
            if (!ok) throw new ExceptionRules("Failed to delete Port Master.");

            // audit/event
            var ev = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: before.PortCode,
                actionName: before.PortName,
                details: $"PortMaster '{before.PortCode} - {before.PortName}' soft-deleted.",
                module: "PortMaster"
            );
            await _mediator.Publish(ev, ct);

            return true;
        }
    }
}
