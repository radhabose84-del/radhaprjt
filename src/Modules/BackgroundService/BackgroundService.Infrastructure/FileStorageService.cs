using BackgroundService.Application.Workflow.Common;
using BackgroundService.Application.Workflow.Common.Interfaces;
using BackgroundService.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;

namespace BackgroundService.Infrastructure
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IHostEnvironment _env;
         private readonly IHttpContextAccessor _httpContext;
         public FileStorageService(IHostEnvironment env, IHttpContextAccessor httpContext)
         {
             _env = env;
            _httpContext = httpContext;
         }

        public async Task<string> GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        public async Task<FileUploadResult> SaveFileAsync(IFormFile file, string subDirectory)
        {
             var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
              var folderPath = Path.Combine(Path.Combine(_env.ContentRootPath, MiscEnumEntity.wwwroot), subDirectory);
              Directory.CreateDirectory(folderPath);
        
              var relativePath = Path.Combine(subDirectory, fileName);
              var fullPath = Path.Combine(folderPath, fileName);
        
               using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
                 {
                     await file.CopyToAsync(stream);
                 }

            //   byte[] fileBytes = await File.ReadAllBytesAsync(fullPath);
            // string base64String = Convert.ToBase64String(fileBytes);
        
              var request = _httpContext.HttpContext?.Request;
              var fileUrl = request is not null
                  ? $"{request.Scheme}://{request.Host}/{relativePath.Replace("\\", "/")}"
                  : relativePath;
        
              return new FileUploadResult
              {
                  FileName = fileName,
                  RelativePath = relativePath.Replace("\\", "/"),
                  Url = fileUrl
                //   Base64 = base64String
              };
        }
    }
}