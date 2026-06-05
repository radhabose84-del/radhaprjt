using MediatR;

namespace PurchaseManagement.Application.RawMaterialPO.Commands.DeleteDocument;

// Deletes a Raw Material PO document by its file name (e.g. a "TEMP_..." upload not yet attached
// to a PO, or an attached "{PONumber}.{ext}" file). If the file is referenced by a PO row, that
// reference is cleared too.
public sealed record DeleteRawMaterialPODocumentCommand(string FileName) : IRequest<bool>;
