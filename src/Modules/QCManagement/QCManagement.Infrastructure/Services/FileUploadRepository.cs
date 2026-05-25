using QCManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace QCManagement.Infrastructure.Services
{
    public class FileUploadRepository : IFileUploadService
    {
        private readonly IHostEnvironment _hostEnvironment;

        public FileUploadRepository(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public async Task<string> UploadFileAsync(string base64File, string fileName, string folderPath)
        {
            var uploadsFolder = Path.Combine(_hostEnvironment.ContentRootPath, folderPath);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var tempFilePath = Path.Combine(uploadsFolder, uniqueFileName);
            var fileBytes = Convert.FromBase64String(base64File);

            await File.WriteAllBytesAsync(tempFilePath, fileBytes);

            return uniqueFileName;
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
