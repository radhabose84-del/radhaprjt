using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Workflow
{
    public class ApproverListDto
    {
        public int ApprovalRequestLineId { get; set; }
        public int ModuleTransactionId { get; set; }
        public string Status { get; set; } = default!;
        public string ApproverBinding { get; set; } = default!;
        public string ApproverValue { get; set; } = default!;
        public int ApprovalRequestId { get; set; }        
        public byte IsEdit { get; set; }
    }
}