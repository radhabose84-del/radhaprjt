using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeAllocation.Command.DeleteBarcodeAllocation
{
    public class DeleteBarcodeAllocationCommandHandler : IRequestHandler<DeleteBarcodeAllocationCommand, bool>
    {
        private readonly IBarcodeAllocationCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteBarcodeAllocationCommandHandler(
            IBarcodeAllocationCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteBarcodeAllocationCommand request, CancellationToken cancellationToken)
        {
            var isDeleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!isDeleted)
                throw new ExceptionRules("Barcode allocation not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "BARCODEALLOCATION_DELETE",
                actionName: request.Id.ToString(),
                details: $"Barcode allocation with Id {request.Id} deleted successfully.",
                module: "BarcodeAllocation"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
