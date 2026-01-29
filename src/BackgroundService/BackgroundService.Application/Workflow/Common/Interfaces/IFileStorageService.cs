using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BackgroundService.Application.Workflow.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<FileUploadResult> SaveFileAsync(IFormFile file, string subDirectory);
        Task<string> GetContentType(string path);
    }
}