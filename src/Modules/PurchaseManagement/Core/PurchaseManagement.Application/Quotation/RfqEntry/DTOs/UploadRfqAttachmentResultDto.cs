namespace PurchaseManagement.Application.Quotation.RfqEntry.DTOs;

public sealed record UploadRfqAttachmentResultDto(
    string FileName,
    string OriginalFileName,
    long FileSize,
    string? FileType);
