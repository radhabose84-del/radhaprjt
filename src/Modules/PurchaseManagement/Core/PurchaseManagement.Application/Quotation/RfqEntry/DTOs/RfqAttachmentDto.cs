namespace PurchaseManagement.Application.Quotation.RfqEntry.DTOs;

public sealed record RfqAttachmentDto(
    int Id,
    int RfqId,
    string FileName,
    string OriginalFileName,
    string? FileType,
    long FileSize);
