using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.DeliveryScoreRule.Commands.DeleteDeliveryScoreRule
{
    public class DeleteDeliveryScoreRuleCommandHandler : IRequestHandler<DeleteDeliveryScoreRuleCommand, bool>
    {
        private readonly IDeliveryScoreRuleCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteDeliveryScoreRuleCommandHandler(
            IDeliveryScoreRuleCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteDeliveryScoreRuleCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);
            if (!result)
                throw new ExceptionRules("DeliveryScoreRule not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "DELIVERY_SCORE_RULE_DELETE",
                actionName: request.Id.ToString(),
                details: $"DeliveryScoreRule with Id {request.Id} deleted successfully.",
                module: "DeliveryScoreRule"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
