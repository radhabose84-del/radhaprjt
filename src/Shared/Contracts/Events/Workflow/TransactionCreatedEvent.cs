using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts.Events.Workflow
{
    public class TransactionCreatedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string ModuleTypeName { get; set; } = default!;
        public int ModuleTransactionId { get; set; }
        public string Payload { get; set; } = default!;
    }
}