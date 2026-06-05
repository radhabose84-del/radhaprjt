using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.DeleteDocument;

public class DeleteRawMaterialPODocumentCommandHandler : IRequestHandler<DeleteRawMaterialPODocumentCommand, bool>
{
    private readonly IRawMaterialPOCommandRepository _commandRepo;
    private readonly IRawMaterialPOFileStorage _storage;
    private readonly IMediator _mediator;

    public DeleteRawMaterialPODocumentCommandHandler(
        IRawMaterialPOCommandRepository commandRepo,
        IRawMaterialPOFileStorage storage,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _storage = storage;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeleteRawMaterialPODocumentCommand request, CancellationToken ct)
    {
        // Accept either a bare file name or a full URL/path.
        var input = request.FileName ?? string.Empty;
        var bareName = Path.GetFileName(input.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(bareName))
            return false;

        // Clear the DB reference if this file is attached to a PO (DB stores the bare file name).
        await _commandRepo.ClearDocumentPathByFileNameAsync(bareName, ct);

        // Delete the physical file (storage maps a full public URL back to its local path,
        // or uses the current unit folder for a bare name).
        var deleted = await _storage.DeleteAsync(input, ct);

        if (!deleted)
            return false;

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Delete",
            actionCode: "RAWMATERIALPO_DOCUMENT_DELETE",
            actionName: bareName,
            details: $"Raw Material PO document '{bareName}' deleted.",
            module: "RawMaterialPO");

        await _mediator.Publish(auditEvent, ct);

        return true;
    }
}
