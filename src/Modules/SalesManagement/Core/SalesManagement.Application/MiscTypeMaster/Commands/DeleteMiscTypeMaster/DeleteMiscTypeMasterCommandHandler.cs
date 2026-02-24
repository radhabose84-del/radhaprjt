#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommandHandler : IRequestHandler<DeleteMiscTypeMasterCommand, bool>
    {
        private readonly IMiscTypeMasterCommandRepository _commandRepository;
        private readonly IMiscTypeMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteMiscTypeMasterCommandHandler(
            IMiscTypeMasterCommandRepository commandRepository,
            IMiscTypeMasterQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMiscTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id);
            if (existing == null)
                throw new ExceptionRules($"Misc Type Master with Id {request.Id} not found.");

            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Failed to delete Misc Type Master.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "MISC_TYPE_DELETE",
                actionName: existing.MiscTypeCode,
                details: $"Misc Type Master '{existing.MiscTypeCode}' deleted successfully.",
                module: "MiscTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
