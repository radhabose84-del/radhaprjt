using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;

public interface IRfqAttachmentFileStorage
{
    Task<StagedRfqAttachment> SaveToStagingAsync(IFormFile file, CancellationToken ct);

    Task<string> MoveStagedToPermanentAsync(string stagedFileName, CancellationToken ct);

    Task DeleteAsync(string? filePath, CancellationToken ct);
}

public sealed record StagedRfqAttachment(
    string FileName,
    string OriginalFileName,
    long FileSize,
    string? FileType);
