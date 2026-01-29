using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.Common
{
    public class FileUploadResult
    {
        public string FileName { get; set; } = default!;
        public string RelativePath { get; set; } = default!;
        public string Url { get; set; } = default!;
        //  public string Base64 { get; set; } = default!;
    }
}