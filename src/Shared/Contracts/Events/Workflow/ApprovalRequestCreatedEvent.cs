using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts.Events.Workflow
{
    public class ApprovalRequestCreatedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}