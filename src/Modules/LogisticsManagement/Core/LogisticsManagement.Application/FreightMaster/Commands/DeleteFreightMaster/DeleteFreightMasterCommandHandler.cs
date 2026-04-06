using Contracts.Common;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Domain.Events;

namespace LogisticsManagement.Application.FreightMaster.Commands.DeleteFreightMaster
{
    public class DeleteFreightMasterCommandHandler : IRequestHandler<DeleteFreightMasterCommand, bool>
    {
        private readonly IFreightMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteFreightMasterCommandHandler(
            IFreightMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteFreightMasterCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("FreightMaster not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "FREIGHT_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"FreightMaster with Id {request.Id} soft deleted.",
                module: "FreightMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
