using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Commands.DeleteVendorEvaluationHeader
{
    public class DeleteVendorEvaluationHeaderCommandHandler : IRequestHandler<DeleteVendorEvaluationHeaderCommand, bool>
    {
        private readonly IVendorEvaluationHeaderCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteVendorEvaluationHeaderCommandHandler(
            IVendorEvaluationHeaderCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteVendorEvaluationHeaderCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);
            if (!result)
                throw new ExceptionRules("VendorEvaluationHeader not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "VENDOR_EVAL_HEADER_DELETE",
                actionName: request.Id.ToString(),
                details: $"VendorEvaluationHeader with Id {request.Id} deleted successfully.",
                module: "VendorEvaluationHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
