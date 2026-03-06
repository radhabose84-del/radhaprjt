using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.Common
{
    public class DownloadFileResult
    {
        public byte[] FileBytes { get; set; } = default!;
         public string ContentType { get; set; } = default!;
         public string FileName { get; set; } = default!;
    }
}