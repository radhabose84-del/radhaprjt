using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.Workflow
{
    public class ApprovalRequestFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
        public int ModuleTransactionId { get; set; }
        public string ModuleTypeName { get; set; }
    }
}