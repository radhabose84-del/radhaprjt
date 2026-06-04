using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeSeries.Command.DeleteBarcodeSeries
{
    public class DeleteBarcodeSeriesCommandHandler : IRequestHandler<DeleteBarcodeSeriesCommand, bool>
    {
        private readonly IBarcodeSeriesCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteBarcodeSeriesCommandHandler(
            IBarcodeSeriesCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteBarcodeSeriesCommand request, CancellationToken cancellationToken)
        {
            var isDeleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!isDeleted)
                throw new ExceptionRules("Barcode series not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "BARCODESERIES_DELETE",
                actionName: request.Id.ToString(),
                details: $"Barcode series with Id {request.Id} deleted successfully.",
                module: "BarcodeSeries"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
