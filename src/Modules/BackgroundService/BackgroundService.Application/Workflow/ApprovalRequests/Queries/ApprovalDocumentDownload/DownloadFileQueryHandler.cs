using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common;
using BackgroundService.Application.Workflow.Common.Interfaces;
using BackgroundService.Domain.Common;
using MediatR;
using Microsoft.Extensions.Hosting;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.ApprovalDocumentDownload
{
    public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, DownloadFileResult>
    {
        private readonly IHostEnvironment _env;
        private readonly IFileStorageService _fileStorageService;
        public DownloadFileQueryHandler(IHostEnvironment env, IFileStorageService fileStorageService)
        {
            _env = env;
            _fileStorageService = fileStorageService;
        }
        public async Task<DownloadFileResult> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
        {
                var fullPath = Path.Combine(
                Path.Combine(_env.ContentRootPath, MiscEnumEntity.wwwroot),
                request.RelativePath
            );

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File not found.", fullPath);

            var contentType = await _fileStorageService.GetContentType(fullPath);
            var fileBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
            var fileName = Path.GetFileName(fullPath);

            return new DownloadFileResult
            {
                FileBytes = fileBytes,
                ContentType = contentType,
                FileName = fileName
            };
        }
    }
}