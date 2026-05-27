using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Commands.DeleteVendorEvaluationCriteria
{
    public class DeleteVendorEvaluationCriteriaCommandHandler : IRequestHandler<DeleteVendorEvaluationCriteriaCommand, bool>
    {
        private readonly IVendorEvaluationCriteriaCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteVendorEvaluationCriteriaCommandHandler(
            IVendorEvaluationCriteriaCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteVendorEvaluationCriteriaCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("VendorEvaluationCriteria not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "VENDOR_EVAL_CRITERIA_DELETE",
                actionName: request.Id.ToString(),
                details: $"VendorEvaluationCriteria with Id {request.Id} deleted successfully.",
                module: "VendorEvaluationCriteria"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
