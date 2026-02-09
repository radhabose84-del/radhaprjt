using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.Mappings;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.UploadDocument
{
    public class DocumentDto : IMapFrom<ProjectDocument>

    {
        public string? FileName { get; set; }
        public string? PODocumentBase64 { get; set; }
    }
}