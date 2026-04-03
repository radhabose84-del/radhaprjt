using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingHeader.Commands.DeleteRepackingHeader
{
    public class DeleteRepackingHeaderCommandHandler
        : IRequestHandler<DeleteRepackingHeaderCommand, bool>
    {
        private readonly IRepackingHeaderCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteRepackingHeaderCommandHandler(
            IRepackingHeaderCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(
            DeleteRepackingHeaderCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("RepackingHeader not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "REPACKING_DELETE",
                actionName: request.Id.ToString(),
                details: $"RepackingHeader with Id {request.Id} soft-deleted successfully.",
                module: "RepackingHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
