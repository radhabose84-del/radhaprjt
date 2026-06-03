using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Commands.DeleteOCREntry
{
    public class DeleteOCREntryCommandHandler : IRequestHandler<DeleteOCREntryCommand, bool>
    {
        private readonly IOCREntryCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteOCREntryCommandHandler(
            IOCREntryCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteOCREntryCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!deleted)
                throw new ExceptionRules("OCR not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "OCR_DELETE",
                actionName: request.Id.ToString(),
                details: $"OCR with Id {request.Id} deleted successfully.",
                module: "OCREntry");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
