using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveDocumentUpload
{
    public class UploadFileCommand : IRequest<FileUploadResult>
    {
        public IFormFile File { get; set; }
    }
}