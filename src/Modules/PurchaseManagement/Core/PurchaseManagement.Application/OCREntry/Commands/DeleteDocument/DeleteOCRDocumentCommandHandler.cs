using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Commands.DeleteDocument;

public class DeleteOCRDocumentCommandHandler : IRequestHandler<DeleteOCRDocumentCommand, bool>
{
    private readonly IOCREntryCommandRepository _commandRepo;
    private readonly IOCREntryFileStorage _storage;
    private readonly IMediator _mediator;

    public DeleteOCRDocumentCommandHandler(
        IOCREntryCommandRepository commandRepo,
        IOCREntryFileStorage storage,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _storage = storage;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeleteOCRDocumentCommand request, CancellationToken ct)
    {
        // Accept either a bare file name or a full URL/path.
        var input = request.FileName ?? string.Empty;
        var bareName = Path.GetFileName(input.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(bareName))
            return false;

        // Clear the DB reference if this file is attached to an OCR (DB stores the bare file name).
        await _commandRepo.ClearDocumentPathByFileNameAsync(bareName, ct);

        // Delete the physical file. Storage maps a full public URL back to its local path
        // (Resources/Purchase/OCRDocuments/{company}/{unit}/...), or uses the unit folder for a bare name.
        var deleted = await _storage.DeleteAsync(input, ct);

        if (!deleted)
            return false;

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Delete",
            actionCode: "OCR_DOCUMENT_DELETE",
            actionName: bareName,
            details: $"OCR document '{bareName}' deleted.",
            module: "OCREntry");

        await _mediator.Publish(auditEvent, ct);

        return true;
    }
}
