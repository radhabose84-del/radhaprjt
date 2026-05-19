using Microsoft.AspNetCore.Http;

namespace GateEntryManagement.Application.Common.Interfaces.IGateInward
{
    public interface IGateInwardAttachmentFileStorage
    {
        Task<StagedGateInwardAttachment> SaveToStagingAsync(IFormFile file, CancellationToken ct);

        // Moves a staged file into the permanent misc-configured sub-folder,
        // naming it after the sanitized desiredBaseName (e.g. the GateEntryNo).
        // Returns the path relative to the Resources root (e.g. "GateEntry/GE-2025-0005.pdf").
        Task<string> MoveStagedToPermanentAsync(
            string stagedFileName, string subFolder, string desiredBaseName, CancellationToken ct);

        Task DeleteAsync(string? relativePath, CancellationToken ct);
    }

    public sealed record StagedGateInwardAttachment(
        string FileName,
        string OriginalFileName,
        long FileSize,
        string? FileType);
}
