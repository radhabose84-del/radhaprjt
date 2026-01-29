using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class ApprovalDocument 
    {
        public int Id { get; set; }
         public int ApprovalRequestId { get; set; }
         public string FileName { get; set; }
         public string FilePath { get; set; }
         public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
         public ApprovalRequest ApprovalRequest { get; set; }
    }
}