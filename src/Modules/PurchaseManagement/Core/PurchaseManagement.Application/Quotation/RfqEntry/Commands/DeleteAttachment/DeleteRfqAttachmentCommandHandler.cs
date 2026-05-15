using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Commands.DeleteAttachment;

public class DeleteRfqAttachmentCommandHandler : IRequestHandler<DeleteRfqAttachmentCommand, bool>
{
    private readonly IRfqCommandRepository _commandRepo;
    private readonly IRfqAttachmentFileStorage _storage;
    private readonly IMediator _mediator;

    public DeleteRfqAttachmentCommandHandler(
        IRfqCommandRepository commandRepo,
        IRfqAttachmentFileStorage storage,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _storage = storage;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeleteRfqAttachmentCommand request, CancellationToken ct)
    {
        var filePath = await _commandRepo.SoftDeleteAttachmentAsync(
            request.RfqId, request.AttachmentId, ct);

        if (filePath == null)
            return false;

        await _storage.DeleteAsync(filePath, ct);

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "SoftDelete",
            actionCode: "RFQ_ATTACHMENT_DELETE",
            actionName: request.AttachmentId.ToString(),
            details: $"RFQ attachment {request.AttachmentId} (RFQ {request.RfqId}) deleted.",
            module: "RFQ");

        await _mediator.Publish(auditEvent, ct);

        return true;
    }
}
