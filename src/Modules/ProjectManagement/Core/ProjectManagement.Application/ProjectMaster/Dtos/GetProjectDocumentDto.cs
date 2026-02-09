using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster
{
    public class GetProjectDocumentDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int DocumentId { get; set; }
        public string FileName { get; set; } = null!;
        public DateTimeOffset UploadedDate { get; set; }
        public string? UploadedPath { get; set; }
        public string? DocumentName { get; set; } 
    }
}