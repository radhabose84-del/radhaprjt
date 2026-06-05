using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Application.Common.Interfaces.IRawMaterialPO;

public interface IRawMaterialPOFileStorage
{
    /// <summary>
    /// Saves the uploaded file into the current company/unit folder under a temporary name
    /// and returns that file name. The file is renamed to the PO number during create/update
    /// via <see cref="RenameAsync"/>.
    /// </summary>
    Task<SavedRawMaterialPODocument> SaveAsync(IFormFile file, CancellationToken ct);

    /// <summary>
    /// Renames an already-saved file (in the current unit folder) to "{poNumber}{ext}"
    /// and returns the new file name. Existing target is overwritten.
    /// </summary>
    Task<string> RenameAsync(string currentFileName, string poNumber, CancellationToken ct);

    /// <summary>Deletes a file (by name or full URL) from the current company/unit folder. Returns true when a file was deleted.</summary>
    Task<bool> DeleteAsync(string? fileName, CancellationToken ct);

    /// <summary>
    /// Builds the public URL for a stored document:
    /// {baseUrl}/Purchase/RawMaterialPODocuments/{Company}/{Unit}/{fileName}
    /// (company/unit resolved from the current token). <paramref name="baseUrl"/> is the
    /// ImagePath value from MiscType (e.g. "http://192.168.1.126/Resources/").
    /// </summary>
    Task<string> BuildPublicUrlAsync(string baseUrl, string fileName, CancellationToken ct);
}

public sealed record SavedRawMaterialPODocument(
    string FileName,
    string OriginalFileName,
    long FileSize,
    string? FileType);
