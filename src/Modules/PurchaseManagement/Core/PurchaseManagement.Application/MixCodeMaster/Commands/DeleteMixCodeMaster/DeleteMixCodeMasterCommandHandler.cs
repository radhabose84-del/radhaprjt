using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.MixCodeMaster.Commands.DeleteMixCodeMaster
{
    public class DeleteMixCodeMasterCommandHandler : IRequestHandler<DeleteMixCodeMasterCommand, bool>
    {
        private readonly IMixCodeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteMixCodeMasterCommandHandler(
            IMixCodeMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMixCodeMasterCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);
            if (!deleted)
                throw new ExceptionRules("MixCodeMaster not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "MIXCODEMASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"MixCodeMaster with Id {request.Id} deleted successfully.",
                module: "MixCodeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
