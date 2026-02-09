using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Domain.Common;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace ProjectManagement.Domain.Entities
{
    public class ProjectDocument 
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }       
        public int DocumentId { get; set; }      
        public string FileName { get; set; } = default!;
        public DateTimeOffset UploadedDate { get; set; }
        

        // Navigation
        public ProjectMaster Project { get; set; } = default!;
        
    }
}