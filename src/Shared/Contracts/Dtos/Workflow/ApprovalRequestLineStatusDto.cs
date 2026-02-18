using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Workflow
{
    public class ApprovalRequestLineStatusDto
    {
        public int ModuleLineTransactionId { get; set; }
        public int ApprovalRequestLineTransactionId { get; set; }
        public string Status { get; set; } = default!;
    }
}