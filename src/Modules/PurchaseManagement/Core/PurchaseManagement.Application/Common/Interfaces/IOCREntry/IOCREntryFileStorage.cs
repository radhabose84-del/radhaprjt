using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Application.Common.Interfaces.IOCREntry;

public interface IOCREntryFileStorage
{
    /// <summary>
    /// Saves the uploaded file directly into the current company/unit folder under a
    /// temporary name and returns that file name. The file is renamed to the OCR number
    /// during create/update via <see cref="RenameAsync"/>.
    /// </summary>
    Task<SavedOCRDocument> SaveAsync(IFormFile file, CancellationToken ct);

    /// <summary>
    /// Renames an already-saved file (in the current unit folder) to "{ocrNumber}{ext}"
    /// and returns the new file name. Existing target is overwritten.
    /// </summary>
    Task<string> RenameAsync(string currentFileName, string ocrNumber, CancellationToken ct);

    /// <summary>Deletes a file (by name) from the current company/unit folder. Returns true when a file was deleted.</summary>
    Task<bool> DeleteAsync(string? fileName, CancellationToken ct);

    /// <summary>
    /// Builds the public URL for a stored document:
    /// {baseUrl}/{ModuleFolder}/{ContainerFolder}/{Company}/{Unit}/{fileName}
    /// (company/unit resolved from the current token). <paramref name="baseUrl"/> is the
    /// OCRPath value from MiscMaster, e.g. "http://192.168.1.126/Resources/".
    /// </summary>
    Task<string> BuildPublicUrlAsync(string baseUrl, string fileName, CancellationToken ct);
}

public sealed record SavedOCRDocument(
    string FileName,
    string OriginalFileName,
    long FileSize,
    string? FileType);
