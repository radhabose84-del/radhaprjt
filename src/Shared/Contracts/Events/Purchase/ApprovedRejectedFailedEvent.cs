using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Purchase;
using Contracts.Dtos.Workflow;

namespace Contracts.Events.Purchase
{
    public class ApprovedRejectedFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public int IndentId { get; set; }
        public string Reason { get; set; }
        public ICollection<UpdateLineStatusDto> LineStatus { get; set; }

    }
}