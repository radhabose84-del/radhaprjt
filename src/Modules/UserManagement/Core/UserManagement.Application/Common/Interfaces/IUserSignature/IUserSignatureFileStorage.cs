using Microsoft.AspNetCore.Http;

namespace UserManagement.Application.Common.Interfaces.IUserSignature
{
    public sealed record SavedUserSignatureFile(
        string FileName,
        string OriginalFileName,
        string FilePath,
        string FileType,
        long FileSize);

    public interface IUserSignatureFileStorage
    {
        Task<SavedUserSignatureFile> SaveAsync(IFormFile file, string userName, int userId, CancellationToken cancellationToken);

        Task DeleteAsync(string? filePath, CancellationToken cancellationToken);

        Task<string?> ReadAsBase64Async(string? filePath, CancellationToken cancellationToken);
    }
}
