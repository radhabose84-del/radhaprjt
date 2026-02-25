
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.BusinessUnit.Commands.DeleteBusinessUnit
{
    public class DeleteBusinessUnitCommandHandler : IRequestHandler<DeleteBusinessUnitCommand, bool>
    {
        private readonly IBusinessUnitCommandRepository _commandRepository;
        private readonly IBusinessUnitQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteBusinessUnitCommandHandler(
            IBusinessUnitCommandRepository commandRepository,
            IBusinessUnitQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteBusinessUnitCommand request, CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id);

            if (existing == null)
                throw new EntityNotFoundException("Business Unit not found");

            var deleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (deleted)
            {
                // Publish audit log event
                var auditEvent = new AuditLogsDomainEvent(
                    actionDetail: "SoftDelete",
                    actionCode: "BUSINESSUNIT_DELETE",
                    actionName: existing.BusinessUnitCode,
                    details: $"Business Unit '{existing.BusinessUnitCode}' deleted successfully.",
                    module: "BusinessUnit"
                );
                await _mediator.Publish(auditEvent, cancellationToken);
            }

            return deleted;
        }
    }
}
