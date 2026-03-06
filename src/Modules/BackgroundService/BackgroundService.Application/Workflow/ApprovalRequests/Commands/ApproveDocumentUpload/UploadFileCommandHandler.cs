using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common;
using BackgroundService.Application.Workflow.Common.Interfaces;
using BackgroundService.Domain.Common;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveDocumentUpload
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, FileUploadResult>
    {
        private readonly IFileStorageService _fileStorageService;
        public UploadFileCommandHandler(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }
        public async Task<FileUploadResult> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
             return await _fileStorageService.SaveFileAsync(request.File, MiscEnumEntity.ApproveDocument);
        }
    }
}