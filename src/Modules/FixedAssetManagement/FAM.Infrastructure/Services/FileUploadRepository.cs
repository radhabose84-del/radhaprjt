#nullable disable
using FAM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FAM.Infrastructure.Services
{
    public class FileUploadRepository : IFileUploadService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public FileUploadRepository(IHttpContextAccessor httpContextAccessor)
        {
           _httpContextAccessor = httpContextAccessor;
        }
        public Task<bool> DeleteFileAsync(string filePath)
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }

        public async Task<(bool IsSuccess, string FilePath, string logoBase64)> UploadFileAsync(IFormFile file, string uploadPath)
        {
            
              string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        
            string tempPath = Path.Combine(uploadPath, "temp");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            string filePath = Path.Combine(tempPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string finalPath = Path.Combine(uploadPath, fileName);
            File.Move(filePath, finalPath);
             string logoBase64 = null;
             if (!string.IsNullOrEmpty(finalPath) && File.Exists(finalPath))
             {
                 byte[] imageBytes = await File.ReadAllBytesAsync(finalPath);
                 logoBase64 = Convert.ToBase64String(imageBytes);
             }
            return (true, finalPath, logoBase64);
        }
    }
}