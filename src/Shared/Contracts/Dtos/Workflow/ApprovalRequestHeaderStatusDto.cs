using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Workflow
{
    public class ApprovalRequestHeaderStatusDto
    {
        public int ModuleTransactionId { get; set; }
        public int ApprovalRequestHeaderTransactionId { get; set; }
        public string Status { get; set; } = default!;
    }
}