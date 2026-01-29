using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Core.Application.Common.Interfaces
{
    public interface IFileUploadService
    {
        Task<(bool IsSuccess, string FilePath, string logoBase64)> UploadFileAsync(IFormFile file, string uploadPath);
        Task<bool> DeleteFileAsync(string filePath);
        // Task<bool> SetFileSession(string value);
        // Task<string> GetFileSession();
    }
}