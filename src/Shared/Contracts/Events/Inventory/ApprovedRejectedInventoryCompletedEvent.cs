using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts.Events.Inventory
{
    public class ApprovedRejectedInventoryCompletedEvent : CorrelatedBy<Guid>
    {
            public Guid CorrelationId { get; init; }
            public int ModuleTransactionId { get; init; }
    }
}