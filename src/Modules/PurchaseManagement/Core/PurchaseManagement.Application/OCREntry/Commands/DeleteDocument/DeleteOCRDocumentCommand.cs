using MediatR;

namespace PurchaseManagement.Application.OCREntry.Commands.DeleteDocument;

// Deletes an OCR document by its file name (e.g. a "TEMP_..." upload not yet attached to an OCR,
// or an attached "{OcrNumber}.{ext}" file). If the file is referenced by an OCR row, that
// reference is cleared too.
public sealed record DeleteOCRDocumentCommand(string FileName) : IRequest<bool>;
