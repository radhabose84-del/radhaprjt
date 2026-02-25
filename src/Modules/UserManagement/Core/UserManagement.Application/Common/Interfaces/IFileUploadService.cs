using Microsoft.AspNetCore.Http;

namespace UserManagement.Application.Common.Interfaces
{
    public interface IFileUploadService
    {
        Task<(bool IsSuccess, string FilePath, string logoBase64)> UploadFileAsync(IFormFile file, string uploadPath);
        Task<bool> DeleteFileAsync(string filePath);
        // Task<bool> SetFileSession(string value);
        // Task<string> GetFileSession();
    }
}