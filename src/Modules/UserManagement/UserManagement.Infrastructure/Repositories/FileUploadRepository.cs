using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Infrastructure.Repositories
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

        // public Task<string> GetFileSession()
        // {
        //     var context = _httpContextAccessor.HttpContext;
        
        //     if (context != null && context.Session != null)
        //     {
        //         var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
        //         var key = $"Companylogo-{userId}";
        //         Log.Information("GetFileSession");
        //         Log.Information(key);
        //         return Task.FromResult(context.Session.GetString(key) ?? "Not Found");
        //     }
        //     return Task.FromResult("Not Found");
        // }

        // public Task<bool> SetFileSession( string value)
        // {
        //     var context = _httpContextAccessor.HttpContext;
        //     if (context != null && context.Session != null)
        //      {
        //         var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //         var key = $"Companylogo-{userId}";
        //         Log.Information("SetFileSession");
        //         Log.Information(key);
        //          context.Session.SetString(key, value);

        //          return Task.FromResult(true);
        //      }

        //      return Task.FromResult(false);
        // }

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