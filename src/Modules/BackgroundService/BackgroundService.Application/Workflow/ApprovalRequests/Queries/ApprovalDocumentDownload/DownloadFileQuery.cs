using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.ApprovalDocumentDownload
{
    public class DownloadFileQuery : IRequest<DownloadFileResult>
    {
        public string RelativePath { get; set; } = default!;
    }
}