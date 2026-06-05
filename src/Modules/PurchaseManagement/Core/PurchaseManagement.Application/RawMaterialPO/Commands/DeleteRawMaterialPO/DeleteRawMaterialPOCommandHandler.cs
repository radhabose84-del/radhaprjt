using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.DeleteRawMaterialPO
{
    public class DeleteRawMaterialPOCommandHandler : IRequestHandler<DeleteRawMaterialPOCommand, bool>
    {
        private readonly IRawMaterialPOCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteRawMaterialPOCommandHandler(
            IRawMaterialPOCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteRawMaterialPOCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!deleted)
                throw new ExceptionRules("Raw Material PO not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "RAWMATERIALPO_DELETE",
                actionName: request.Id.ToString(),
                details: $"Raw Material PO with Id {request.Id} deleted successfully.",
                module: "RawMaterialPO");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
