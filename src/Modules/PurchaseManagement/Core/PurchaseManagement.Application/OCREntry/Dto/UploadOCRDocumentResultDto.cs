namespace PurchaseManagement.Application.OCREntry.Dto;

// FileName is the value the client must send back as Create/UpdateOCREntryCommand.DocumentPath.
// On create/update the file is renamed to "{OcrNumber}{ext}".
public sealed record UploadOCRDocumentResultDto(
    string FileName,
    string OriginalFileName,
    long FileSize,
    string? FileType);
