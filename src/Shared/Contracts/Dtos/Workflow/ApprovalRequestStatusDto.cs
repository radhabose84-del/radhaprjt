using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Workflow
{
    public class ApprovalRequestStatusDto
    {
        public int Id { get; set; }
        public int ModuleTransactionId { get; set; }
        public int ApprovalRequestId { get; set; }
        public string CurrentStatus { get; set; }
        public string ModuleTypeName { get; set; }
    }
}