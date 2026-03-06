using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Dto
{
    public class ApproverListItemDto
    {
        public int Id { get; set; }
        public int ModuleTransactionId { get; set; }
        public string? Status { get; set; }
        public string? ApproverBinding { get; set; }
        public string? ApproverValue { get; set; }
        public int ApprovalRequestId { get; set; }
        public byte IsEdit { get; set; }
        
        public StatusDto? Statusdto { get; set; } 
    }
}