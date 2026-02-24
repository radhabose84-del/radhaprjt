#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MiscMaster.Commands.DeleteMiscMaster
{
    public class DeleteMiscMasterCommandHandler : IRequestHandler<DeleteMiscMasterCommand, bool>
    {
        private readonly IMiscMasterCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteMiscMasterCommandHandler(
            IMiscMasterCommandRepository commandRepository,
            IMiscMasterQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMiscMasterCommand request, CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id);
            if (existing == null)
                throw new ExceptionRules($"Misc Master with Id {request.Id} not found.");

            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Failed to delete Misc Master.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "MISC_MASTER_DELETE",
                actionName: existing.Code,
                details: $"Misc Master '{existing.Code}' deleted successfully.",
                module: "MiscMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
