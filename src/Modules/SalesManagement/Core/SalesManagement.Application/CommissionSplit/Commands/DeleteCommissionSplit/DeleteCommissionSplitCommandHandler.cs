using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CommissionSplit.Commands.DeleteCommissionSplit
{
    public class DeleteCommissionSplitCommandHandler : IRequestHandler<DeleteCommissionSplitCommand, bool>
    {
        private readonly ICommissionSplitCommandRepository _commandRepository;
        private readonly ICommissionSplitQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteCommissionSplitCommandHandler(
            ICommissionSplitCommandRepository commandRepository,
            ICommissionSplitQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteCommissionSplitCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("CommissionSplit not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "COMMISSION_SPLIT_DELETE",
                actionName: request.Id.ToString(),
                details: $"CommissionSplit with Id {request.Id} soft deleted successfully.",
                module: "CommissionSplit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
